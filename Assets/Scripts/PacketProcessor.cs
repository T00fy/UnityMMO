using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using MMOServer;
using System.Threading;

public class PacketProcessor : MonoBehaviour {
    public GameObject menuHandlerObj;
  

    private string userName;
    private string password;
    private GameObject cursor;
    private IPAddress[] ip;
    private MenuHandler menuHandler;


    //general case when receiving any packet and deciding what to do with it
    public void ProcessPacket(ClientConnect clientConnection, BasePacket receivedPacket)
    {

    }

    public void LoginOrRegister(string userName, string password) {
        
        this.userName = userName;
        this.password = password;



        BasePacket packetToSend = GetBasePacketSomeHow(); //for logging in and registerin
        EstablishConnection connect = new EstablishConnection();
        connect.Connect();
        connect.Send(packetToSend);



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
        if (password.Length < 4 || userName.Length < 3)
        {
            throw new Exception("Password and Username length must be greater than 4 characters");
        }
    }


    private void OpenStatusBox()
    {
        menuHandler = menuHandlerObj.GetComponent<MenuHandler>();
        cursor = GameObject.Find("Cursor");
        menuHandler.SetCursor(cursor);
        menuHandler.ToggleCursor(false);
        menuHandler.OpenStatusBox();
    }

}
