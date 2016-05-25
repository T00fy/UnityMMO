using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.Threading;


public class StateObject
{
    // Client socket.
    public Socket workSocket = null;
    // Size of receive buffer.
    public const int BufferSize = 256;
    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];
    // Received data string.
    public StringBuilder sb = new StringBuilder();
}

public class ClickRegister : MonoBehaviour {
    public GameObject menuHandlerObj;


    private string userName;
    private string password;
    private GameObject cursor;
    private IPAddress[] ip;
    private static string response = string.Empty;
   
    private static string cmd;
    private static MenuHandler menuHandler;


    
    public void StartConnection(string cmd1, string userName, string password) {
        menuHandler = menuHandlerObj.GetComponent<MenuHandler>();
        cmd = cmd1;
        this.userName = userName;
        this.password = password;

        cursor = GameObject.Find("Cursor");
        menuHandler.SetCursor(cursor);
        menuHandler.ToggleCursor(false);
        menuHandler.OpenStatusBox();
        

        RegisterConnection();





    }


    private void CheckInputs()
    {
        if (password.Contains(" ") || userName.Contains(" "))
        {
            throw new Exception("Invalid character in Username or Password");
        }
        if (password == null && userName == null) {
            throw new Exception("Empty username or password");
        }
        if (password.Length < 4 || userName.Length < 3) {
            throw new Exception("Password and Username length must be greater than 4 characters");
        }
    }

    bool SocketConnected(Socket s)
    {
        bool part1 = s.Poll(1000, SelectMode.SelectRead);
        bool part2 = (s.Available == 0);
        if ((part1 && part2) || !s.Connected)
            return false;
        else
            return true;
    }

    private void RegisterConnection() {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            CheckInputs();
            ip = Dns.GetHostAddresses("127.0.0.1");
            
            
            menuHandler.SetStatusText("Connecting...");

            IPEndPoint remoteEP = new IPEndPoint(ip[0],3425);
            socket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallBack), socket);
            
        }
        catch (Exception e)
        {
            menuHandler.DestroyStatusBox();
            menuHandler.SetStatusText(e.Message);
            Debug.Log(e.Message);
        }

    }

    private static void ConnectCallBack(IAsyncResult aSyncResult)
    {
        Socket socket = (Socket)aSyncResult.AsyncState;
        try
        {
            
            socket.EndConnect(aSyncResult);
            menuHandler.SetStatusText("Connected!");
            
            Send(socket, cmd);
        }
        catch (Exception e)
        {
            menuHandler.DestroyStatusBox();
            menuHandler.SetStatusText(e.Message);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }


    }



    private static void Send(Socket client, string data)
    {
        // Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.
        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallBack), client);
    }

    private static void SendCallBack(IAsyncResult aSyncResult)
    {
        Socket socket = (Socket)aSyncResult.AsyncState;
        socket.EndSend(aSyncResult);
        

        Receive(socket);
    }



    private static void Receive(Socket client)
    {

        try
        {
            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = client;

            // Begin receiving the data from the remote device.
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallBack), state);
        }
        catch (Exception e)
        {
            menuHandler.DestroyStatusBox();
            menuHandler.SetStatusText(e.Message);
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }

    private static void ReceiveCallBack(IAsyncResult aSyncResult)
    {
        StateObject state = (StateObject)aSyncResult.AsyncState;
        Socket socket = state.workSocket; //the callback socket
        try {

            int received = socket.EndReceive(aSyncResult); //number of bytes received

            if (received > 0)
            {
                // There might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, received));

                // Get the rest of the data.
                socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallBack), state);
            }
            else {
                // All the data has arrived; put it in response.
                if (state.sb.Length > 1)
                {
                    menuHandler.DestroyStatusBox();
                    response = state.sb.ToString();
                    menuHandler.SetStatusText(response);

                    if (response == "Login successful") {
                        //set some boolean to true
                        //deactivate all other game objects
                        //Show character selection screen
                        //create a character server that has all character options

                    }
                }
                // Signal that all bytes have been received.
                CloseSocket(socket);
     //           receiveDone.Set();
            }
        }
        catch(Exception e) {
            menuHandler.DestroyStatusBox();
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            Debug.Log(e.ToString());
        }


        
    }

    private static void CloseSocket(Socket socket)
    {
        menuHandler.DestroyStatusBox();
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }
}
