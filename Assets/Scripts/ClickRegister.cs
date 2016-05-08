using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;



//
//
//CLASS DOES NOT WORK TOO WELL, SOCKET TENDS TO GET STUCK IN AN INFINITE LOOP TRYING TO RECEIVE DATA
//
//


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
    private TextFieldHandler usernameEntryHandler;
    private TextFieldHandler passwordEntryHandler;
    private Button regButton;
    private Button loginButton;
    private string userName;
    private string password;
    private IPAddress[] ip;
    private static String response = String.Empty;
    private static ManualResetEvent connectDone =
    new ManualResetEvent(false);
    private static ManualResetEvent sendDone =
        new ManualResetEvent(false);
    private static ManualResetEvent receiveDone =
        new ManualResetEvent(false);



    void Start() {
        GameObject passwordGameObj = GameObject.FindWithTag("PasswordEntry");
        passwordEntryHandler = passwordGameObj.GetComponent<TextFieldHandler>();

        GameObject userGameObj = GameObject.FindWithTag("UsernameEntry");
        usernameEntryHandler = userGameObj.GetComponent<TextFieldHandler>();

        GameObject regButtonGameObj = GameObject.Find("Register");
        regButton = regButtonGameObj.GetComponent<Button>();

        GameObject loginGameObj = GameObject.Find("Login");
        loginButton = loginGameObj.GetComponent<Button>();

        regButton.onClick.AddListener(RegisterConnection);

        loginButton.onClick.AddListener(LoginConnection);




    }

    private void LoginConnection()
    {
        throw new NotImplementedException();
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
        if (password.Length < 4) {
            throw new Exception("Password length must be greater than 4 characters");
        }
    }

    private void RegisterConnection() {
        password = passwordEntryHandler.GetInput();
        userName = usernameEntryHandler.GetInput();
        try
        {
            CheckInputs();
            ip = Dns.GetHostAddresses("127.0.0.1");
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //socket.Connect(ip[0], 3425);
            socket.BeginConnect(ip[0], 3425, new AsyncCallback(ConnectCallBack), socket);
            connectDone.WaitOne();

            String cmd = "register " + userName + " " + password;
            Send(socket, cmd);
            sendDone.WaitOne();

            Receive(socket);
            receiveDone.WaitOne();


            socket.Shutdown(SocketShutdown.Both);
            socket.Close();


        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }

    }

    private static void Send(Socket client, String data)
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
        int bytesSent = socket.EndSend(aSyncResult);


        

        sendDone.Set();
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
            Console.WriteLine(e.ToString());
        }
    }

    private static void ReceiveCallBack(IAsyncResult aSyncResult)
    {
        try {
            Debug.Log("Receiving");
            StateObject state = (StateObject)aSyncResult.AsyncState;
            Socket socket = state.workSocket; //the callback socket
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
                    response = state.sb.ToString();
                    Debug.Log("Received status: " + response);
                }
                // Signal that all bytes have been received.
                receiveDone.Set();
            }
        }
        catch(Exception e) {
            Debug.Log(e.ToString());
        }


        
    }

    private static void ConnectCallBack(IAsyncResult aSyncResult)
    {
        try
        {
            Socket socket = (Socket)aSyncResult.AsyncState;
            socket.EndConnect(aSyncResult);


            connectDone.Set();
        }
        catch (Exception e) {
            Debug.Log(e.ToString());
        }

        //      connectSocket.EndConnect(aSyncResult);
    }




}
