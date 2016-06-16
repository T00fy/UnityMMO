using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using MMOServer;
using System.Threading;

//TODO: REFACTOR THIS SO IT'S A GENERIC CONNECTION CLASS
//PASS IN DELEGATES AS PARAMETERS FOR THE CALLBACKS

public class ClickAccountSubmit : MonoBehaviour {
    public GameObject menuHandlerObj;
  

    private string userName;
    private string password;
    private GameObject cursor;
    private IPAddress[] ip;
    private static string response = string.Empty;
   
    private static string cmd;


    
    public void Start(string cmd1, string userName, string password) {
        MenuHandler menuHandler = menuHandlerObj.GetComponent<MenuHandler>();
        cmd = cmd1;
        this.userName = userName;
        this.password = password;

        cursor = GameObject.Find("Cursor");
        menuHandler.SetCursor(cursor);
        menuHandler.ToggleCursor(false);
        menuHandler.OpenStatusBox();

        BasePacket packetToSend = GetBasePacketSomeHow(); //for logging in and registerin
        EstablishConnection connect = new EstablishConnection(packetToSend, menuHandler);
        connect.Connect();



    }
}
