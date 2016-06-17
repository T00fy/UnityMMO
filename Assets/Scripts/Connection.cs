using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using MMOServer;
using System.Threading;

public class Connection {
    public static Socket socket;
    public const int BUFFER_SIZE = 65535;

    private MenuHandler menuHandler;
  //  private BasePacket packetToSend;
    private bool loggedIn;
    private ClientConnect clientConnection;


    //used mainly for logging in and registering, will display a statusbox with packet 

    /*    public EstablishConnection(BasePacket packetToSend, MenuHandler menuHandler) {
            this.packetToSend = packetToSend;
            loggedIn = packetToSend.isAuthenticated();
            this.menuHandler = menuHandler;
        }

        public EstablishConnection(BasePacket packetToSend)
        {
            this.packetToSend = packetToSend;
            loggedIn = packetToSend.isAuthenticated();
        }*/

    //check that connection is already established
    //if not establish connection
    //send packet and then start receiving packets
    //get subpackets
    //store into basepacket and send when ready
    //

    public Connection(MenuHandler statusBox)
    {
        menuHandler = statusBox;
        clientConnection = new ClientConnect();
    }

    public void EstablishConnection()
    {

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            IPAddress[] ip = Dns.GetHostAddresses("127.0.0.1");


            menuHandler.SetStatusText("Connecting...");

            IPEndPoint remoteEP = new IPEndPoint(ip[0], 3425);
         //   socket.Connect(remoteEP, new AsyncCallback(ConnectCallBack), socket);
            socket.Connect(remoteEP);

            clientConnection.socket = socket;
            clientConnection.buffer = new byte[BUFFER_SIZE];
            menuHandler.SetStatusText("Established Connection");

        }
        catch (Exception e)
        {
            menuHandler.SetDestroyStatusBox();
            menuHandler.SetStatusText(e.Message);
            Debug.Log(e);
        }

    }

 /*   private void ConnectCallBack(IAsyncResult aSyncResult)
    {
        clientConnection = new ClientConnect();
        try
        {
            socket = (Socket)aSyncResult.AsyncState;
            
            clientConnection.socket = socket.EndAccept(aSyncResult);

        }
        catch (Exception e)
        {
            menuHandler.SetDestroyStatusBox();
            menuHandler.SetStatusText(e.Message);
            Debug.Log(e);
            clientConnection.socket.Shutdown(SocketShutdown.Both);
            clientConnection.socket.Close();
        }


    }*/

    public void Send(BasePacket packetToSend)
    {

        clientConnection.socket.BeginSend(packetToSend.data, 0, packetToSend.data.Length, 0,
            new AsyncCallback(SendCallBack), clientConnection.socket);
    }

    private void SendCallBack(IAsyncResult aSyncResult)
    {
        clientConnection.socket = (Socket)aSyncResult.AsyncState;
        clientConnection.socket.EndSend(aSyncResult);


        Receive(clientConnection);
    }



    private void Receive(ClientConnect clientConnection)
    {

        try
        {
            // Create the state object.
            // Begin receiving the data from the remote device.
            clientConnection.socket.BeginReceive(clientConnection.buffer, 0, clientConnection.buffer.Length, 0,
                new AsyncCallback(ReceiveCallBack), clientConnection.socket);
        }
        catch (Exception e)
        {
            menuHandler.SetDestroyStatusBox();
            menuHandler.SetStatusText(e.Message);
            Debug.Log(e);
            clientConnection.socket.Shutdown(SocketShutdown.Both);
            clientConnection.socket.Close();
        }
    }

    private void ReceiveCallBack(IAsyncResult aSyncResult)
    {
        ClientConnect clientConnection = (ClientConnect)aSyncResult.AsyncState;
        PacketProcessor packetProcessor = new PacketProcessor();
        try
        {
            int bytesRead = clientConnection.socket.EndReceive(aSyncResult);


            if (bytesRead > 0)
            {
                int offset = 0;

                //build/compile packets until can no longer or data is finished
                while (true)
                {
                    BasePacket basePacket = BuildPacket(ref offset, clientConnection.buffer, bytesRead);
                    if (basePacket == null)
                    {
                        break;
                    }
                    else
                    {

                        packetProcessor.ProcessPacket(clientConnection, basePacket);
                    }

                }
                //Not all bytes consumed, transfer leftover to beginning
                if (offset < bytesRead)
                {
                    Array.Copy(clientConnection.buffer, offset, clientConnection.buffer, 0, bytesRead - offset);
                }

                Array.Clear(clientConnection.buffer, bytesRead - offset, clientConnection.buffer.Length - (bytesRead - offset));

                if (offset < bytesRead)
                //need offset since not all bytes consumed
                {
                    clientConnection.socket.BeginReceive(clientConnection.buffer, bytesRead - offset, clientConnection.buffer.Length - (bytesRead - offset), SocketFlags.None, new AsyncCallback(ReceiveCallBack), clientConnection);
                }
                else
                {
                    clientConnection.socket.BeginReceive(clientConnection.buffer, 0, clientConnection.buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), clientConnection);
                }
            }
            else
            {
                Debug.Log("Lost connection to server");
            }



        }
        catch {
            Debug.Log("something went wrong");
        }
            
   }

        /// <summary>
    /// Builds a packet from the incoming buffer + offset. If a packet can be built, it is returned else null.
    /// </summary>
    /// <param name="offset">Current offset in buffer.</param>
    /// <param name="buffer">Incoming buffer.</param>
    /// <returns>Returns either a BasePacket or null if not enough data.</returns>
    public BasePacket BuildPacket(ref int offset, byte[] buffer, int bytesRead)
    {
        BasePacket newPacket = null;

        //Too small to even get length
        if (bytesRead <= offset)
            return null;

        ushort packetSize = BitConverter.ToUInt16(buffer, offset);

        //Too small to whole packet
        if (bytesRead < offset + packetSize)
            return null;

        if (buffer.Length < offset + packetSize)
            return null;

        try
        {
            newPacket = new BasePacket(buffer, ref offset);
        }
        catch (OverflowException)
        {
            return null;
        }

        return newPacket;
    }

    private void CloseSocket(Socket socket)
    {
        menuHandler.SetDestroyStatusBox();
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }


}
