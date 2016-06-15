using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using MMOServer;
using System.Threading;

public class Connection : MonoBehaviour {
    private static MenuHandler menuHandler;
    private BasePacket packetToSend;
    private bool loggedIn;
    private bool registering;
    private bool loggingIn;

    public Connection(BasePacket packetToSend, MenuHandler menuHandler, bool registering, bool loggingIn) {
        this.packetToSend = packetToSend;
        loggedIn = packetToSend.isAuthenticated();
        this.registering = registering;
        this.loggingIn = loggingIn;
    }

        //one class should just establish connection (ie this one)
        //another class will act as a means to just send using the selected socket, might have to get socket from this class and pass it
        //get subpackets
        //store into basepacket and send when ready
        //

    private void RegisterConnection()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            CheckInputs();
            IPAddress[] ip = Dns.GetHostAddresses("127.0.0.1");


            menuHandler.SetStatusText("Connecting...");

            IPEndPoint remoteEP = new IPEndPoint(ip[0], 3425);
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
            menuHandler.SetStatusText("Established Connection");

            Send(socket, packetToSend);
        }
        catch (Exception e)
        {
            menuHandler.DestroyStatusBox();
            menuHandler.SetStatusText(e.Message);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }


    }


    //redo this so it uses a separate class to queue sends
    private static void Send(Socket client, BasePacket packetToSend)
    {
        // Convert the string data to byte data using ASCII encoding.

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
        try
        {

            int received = socket.EndReceive(aSyncResult); //number of bytes received

            if (received > 0)
            {
                // There might be more data, so store the data received so far.
                state.sb.Append(Encoding.Unicode.GetString(state.buffer, 0, received));

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
                    Debug.Log(response);
                    if (response == "Login Successful")
                    {

                        Debug.Log("success got through");
                        //set some boolean to true
                        //deactivate all other game objects
                        //Show character selection screen
                        //create a character server that has all character options

                    }
                    if (response == "test")
                    {
                        Debug.Log("Test got through");
                    }

                    /*
                     * 
                     * TODO: Object disposed error when sending twice( (probably can just eliminate this by only ever calling begin send once with a working packet protocol)
                     * 
                     */




                }
                // Signal that all bytes have been received.
                CloseSocket(socket);
                //           receiveDone.Set();
            }
        }
        catch (Exception e)
        {
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

    bool SocketConnected(Socket s)
    {
        bool part1 = s.Poll(1000, SelectMode.SelectRead);
        bool part2 = (s.Available == 0);
        if ((part1 && part2) || !s.Connected)
            return false;
        else
            return true;
    }
}
