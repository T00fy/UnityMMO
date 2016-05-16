using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;


//might need to setup coroutines

public class ClickRegisterSynchronous : MonoBehaviour {
    public GameObject statusBox;

    private InputField usernameInput;
    private InputField passwordInput;
    private Button submit;
    private bool submitted;
    // Use this for initialization

    public void StartConnection()
    {
        GameObject passwordGameObj = GameObject.Find("PasswordRegister");
        passwordInput = passwordGameObj.GetComponent<InputField>();

        GameObject userGameObj = GameObject.Find("UsernameRegister");
        usernameInput = userGameObj.GetComponent<InputField>();

        Instantiate(statusBox, new Vector3(0, 0, 0), Quaternion.identity);
        RegisterConnection();

        
    }

    private void CheckInputs(string userName, string password)
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
        string password = passwordInput.text;
        string userName = usernameInput.text;
        byte[] bytesReceived = new byte[1024];
        try
        {
            CheckInputs(userName, password);
            IPAddress[] ip = Dns.GetHostAddresses("127.0.0.1");
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(ip[0], 3425);
            byte[] sendCmd = Encoding.ASCII.GetBytes("register " + userName + " " + password);

            socket.Send(sendCmd);


           int totalBytes = socket.Receive(bytesReceived);

            Debug.Log("RESPONSE: " + Encoding.ASCII.GetString(bytesReceived, 0, totalBytes));


        }
        catch (Exception e) {
            Debug.Log(e.ToString());
        }
    }
}
