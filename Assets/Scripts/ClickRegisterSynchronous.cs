using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class ClickRegisterSynchronous : MonoBehaviour {
    private TextFieldHandler usernameEntryHandler;
    private TextFieldHandler passwordEntryHandler;
    private Button regButton;
    private Button loginButton;
    private string userName;
    private string password;
    // Use this for initialization
    void Start()
    {
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
        if (password == null && userName == null)
        {
            throw new Exception("Empty username or password");
        }
        if (password.Length < 4)
        {
            throw new Exception("Password length must be greater than 4 characters");
        }
    }

    private void RegisterConnection()
    {
        password = passwordEntryHandler.GetInput();
        userName = usernameEntryHandler.GetInput();
        byte[] bytesReceived = new byte[1024];
        try
        {
            CheckInputs();
            IPAddress[] ip = Dns.GetHostAddresses("127.0.0.1");
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(ip[0], 3425);
            byte[] sendCmd = Encoding.ASCII.GetBytes("register " + userName + " " + password);

            int bytesSent = socket.Send(sendCmd);


           int totalBytes = socket.Receive(bytesReceived);

            Debug.Log("RESPONSE: " + Encoding.ASCII.GetString(bytesReceived, 0, totalBytes));


        }
        catch (Exception e) {
            Debug.Log(e.ToString());
        }
    }
}
