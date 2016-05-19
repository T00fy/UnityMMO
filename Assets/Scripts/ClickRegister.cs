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
    public GameObject statusBoxPrefab;

    private Text statusTextObj;
    private string statusText;
    private GameObject cursor;
    private static string userName;
    private static string password;
    private IPAddress[] ip;
    private static string response = string.Empty;
    private bool boxOpened;
    private static StatusBoxHandler statusHandler;
    private GameObject status;

    private static ManualResetEvent connectDone =
    new ManualResetEvent(false);
    private static ManualResetEvent sendDone =
        new ManualResetEvent(false);
    private static ManualResetEvent receiveDone =
        new ManualResetEvent(false);


    
    public void StartConnection() {
        statusText = "";
        GameObject passwordGameObj = GameObject.Find("PasswordRegister");
        InputField passwordInput = passwordGameObj.GetComponent<InputField>();

        GameObject userGameObj = GameObject.Find("UsernameRegister");
        InputField usernameInput = userGameObj.GetComponent<InputField>();

        

        cursor = GameObject.Find("Cursor");
        password = passwordInput.text;
        userName = usernameInput.text;

        //       canvas = statusBox.GetComponentInParent<Canvas>();
        ActivateCursorOnRegister(false);
        status = Instantiate(statusBoxPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        boxOpened = true;
        statusHandler = status.GetComponentInChildren<StatusBoxHandler>();

        statusTextObj = status.GetComponentInChildren<Text>();
        
        //      tempText.transform.SetParent(canvas.transform, false);
        RegisterConnection();





    }

    public void ActivateCursorOnRegister(bool enable)
    {
        cursor.SetActive(enable);
        
    }

    void Update()
    {
        if (statusTextObj != null)
        {
            statusTextObj.text = "Status: " + statusText + ".";
            statusTextObj.text = "Status: " + statusText + "..";
            statusTextObj.text = "Status: " + statusText + "...";
        }
        if (boxOpened && status == null) {
            ActivateCursorOnRegister(true);

        }

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
            
            
            statusText = "Connecting...";

            IPEndPoint remoteEP = new IPEndPoint(ip[0],3425);
            socket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallBack), socket);
            bool connected = SocketConnected(socket);
            if (!connected)
            {
                throw new Exception("Could not connect");

            }
            
        }
        catch (Exception e)
        {
            statusHandler.SetFinished(true);
            statusText = e.Message;
            Debug.Log(e.Message);
        }

    }

    private static void ConnectCallBack(IAsyncResult aSyncResult)
    {
        Socket socket = (Socket)aSyncResult.AsyncState;
        try
        {
            
            socket.EndConnect(aSyncResult);

            connectDone.Set();
            string cmd = "register " + userName + " " + password;
            Send(socket, cmd);
        }
        catch (Exception e)
        {
            statusHandler.SetFinished(true);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            Debug.Log(e.ToString());
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
        
        sendDone.Set();
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
            statusHandler.SetFinished(true);
            client.Shutdown(SocketShutdown.Both);
            client.Close();
            Console.WriteLine(e.ToString());
        }
    }

    private static void ReceiveCallBack(IAsyncResult aSyncResult)
    {
        StateObject state = (StateObject)aSyncResult.AsyncState;
        Socket socket = state.workSocket; //the callback socket
        try {
            Debug.Log("Receiving");

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
                    statusHandler.SetFinished(true);
                    response = state.sb.ToString();
                    Debug.Log("Received status: " + response);
                }
                // Signal that all bytes have been received.
                CloseSocket(socket);
     //           receiveDone.Set();
            }
        }
        catch(Exception e) {
            statusHandler.SetFinished(true);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            Debug.Log(e.ToString());
        }


        
    }

    private static void CloseSocket(Socket socket)
    {
        statusHandler.SetFinished(true);
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }
}
