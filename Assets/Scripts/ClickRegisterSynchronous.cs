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

    private Text statusTextObj;
    private string statusText;
    private bool submitted;
    private string userName;
    private string password;
    
    // Use this for initialization

    public void StartConnection()
    {
        statusText = "";
        GameObject passwordGameObj = GameObject.Find("PasswordRegister");
        InputField passwordInput = passwordGameObj.GetComponent<InputField>();

        GameObject userGameObj = GameObject.Find("UsernameRegister");
        InputField usernameInput = userGameObj.GetComponent<InputField>();
        password = passwordInput.text;
        userName = usernameInput.text;

        //       canvas = statusBox.GetComponentInParent<Canvas>();
        statusTextObj = statusBox.GetComponentInChildren<Text>();
        Instantiate(statusBox, new Vector3(0, 0, 0), Quaternion.identity);
  //      tempText.transform.SetParent(canvas.transform, false);
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

    void Update() {
        if (statusTextObj != null) {
            statusTextObj.text = "Status: " + statusText;
        }
        
    }

    private IEnumerator RegisterConnection()
    {
        yield return null;
        byte[] bytesReceived = new byte[1024];
        try
        {
            CheckInputs(userName, password);
            IPAddress[] ip = Dns.GetHostAddresses("127.0.0.1");
            statusText = "FTP...";
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(ip[0], 3425);
            byte[] sendCmd = Encoding.ASCII.GetBytes("register " + userName + " " + password);
            socket.Send(sendCmd);


           int totalBytes = socket.Receive(bytesReceived);

            Debug.Log("RESPONSE: " + Encoding.ASCII.GetString(bytesReceived, 0, totalBytes));
            statusText =  Encoding.ASCII.GetString(bytesReceived, 0, totalBytes);
        }
        catch (SocketException e) {
            Debug.Log(e.ToString());
            statusText = "Could not connect";
            yield break;
        }
    }
}
