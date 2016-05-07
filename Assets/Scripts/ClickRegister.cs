using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System;
using System.Runtime.Serialization;
using System.Text;

public class ClickRegister : MonoBehaviour {
    private TextFieldHandler usernameEntryHandler;
    private TextFieldHandler passwordEntryHandler;
    private Button regButton;
    private Button loginButton;
    private string userName;
    private string password;
    private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private IPAddress[] ip;
    private static byte[] receiveBuff = new byte[512];



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
            byte[] buffer = Encoding.ASCII.GetBytes("register " + userName + " " + password);
            
            //socket.Connect(ip[0], 3425);
            socket.BeginConnect(ip[0], 3425, new AsyncCallback(ConnectCallBack), null);
            
            socket.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(SendCallBack), socket);

            socket.BeginReceive(receiveBuff, 0, receiveBuff.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
        }
        catch (Exception e)
        {
            var error = e.Message;
            Debug.Log(error);
        }

    }

    private void ReceiveCallBack(IAsyncResult aSyncResult)
    {
        Socket socket = (Socket)aSyncResult.AsyncState; //the callback socket
        int received = socket.EndReceive(aSyncResult); //number of bytes received
        byte[] dataBuf = new byte[received];
        Array.Copy(receiveBuff, dataBuf, received);

        string text = Encoding.ASCII.GetString(dataBuf);

        Debug.Log("Received status: ");
    }

    private void ConnectCallBack(IAsyncResult aSyncResult)
    {
        Socket receiveSocket = (Socket)aSyncResult.AsyncState;
        receiveSocket.EndConnect(aSyncResult);
    }

    private void SendCallBack(IAsyncResult aSyncResult)
    {
        Socket receiveSocket = (Socket)aSyncResult.AsyncState;
    }
}
