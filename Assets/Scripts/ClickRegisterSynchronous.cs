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
    public Text statusText;

    private InputField usernameInput;
    private InputField passwordInput;
    private Button submit;
    private bool submitted;
    private Text tempText;
    private Canvas canvas;
    
    // Use this for initialization

    public void StartConnection()
    {
        GameObject passwordGameObj = GameObject.Find("PasswordRegister");
        passwordInput = passwordGameObj.GetComponent<InputField>();

        GameObject userGameObj = GameObject.Find("UsernameRegister");
        usernameInput = userGameObj.GetComponent<InputField>();

        GameObject canvObj = GameObject.Find("MainMenu");
        canvas = canvObj.GetComponent<Canvas>();

        Instantiate(statusBox, new Vector3(0, 0, 0), Quaternion.identity);
        tempText = Instantiate(statusText, new Vector3(0, 0, 0), Quaternion.identity) as Text;
        tempText.transform.SetParent(canvas.transform, false);
        StartCoroutine(RegisterConnection());


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

    private IEnumerator RegisterConnection()
    {
        yield return null;
        tempText.text = "Status: Connecting...";
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
            tempText.text = "Status: Sending...";
            socket.Send(sendCmd);


           int totalBytes = socket.Receive(bytesReceived);

            Debug.Log("RESPONSE: " + Encoding.ASCII.GetString(bytesReceived, 0, totalBytes));
            tempText.text = "Status: Connecting...";
            tempText.text = "Status: " + Encoding.ASCII.GetString(bytesReceived, 0, totalBytes);
        }
        catch (Exception e) {
            Debug.Log(e.ToString());
            tempText.text = "Status: " + e.ToString();
        }
    }
}
