using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using MMOServer;
using System.Threading;
using System.Collections.Generic;

public class Connection : MonoBehaviour{
    private Socket socket;
    [HideInInspector]
    public const int BUFFER_SIZE = 65535;
    [HideInInspector]
    public byte[] buffer = new byte[0xffff];
    //       public CircularBuffer<byte> incomingStream = new CircularBuffer<byte>(1024);
    public Queue<BasePacket> sendPacketQueue = new Queue<BasePacket>(100);
    [HideInInspector]
    public int lastPartialSize = 0;
    private PacketProcessor packetProcessor;
    //  private BasePacket packetToSend;


    //used mainly for logging in and registering, will display a statusbox with packet 

    /*    public EstablishConnection(BasePacket packetToSend, CursorInput.menuHandler CursorInput.menuHandler) {
            this.packetToSend = packetToSend;
            loggedIn = packetToSend.isAuthenticated();
            this.CursorInput.menuHandler = CursorInput.menuHandler;
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

    void Start()
    {
        packetProcessor = GameObject.FindGameObjectWithTag("PacketProcessor").GetComponent<PacketProcessor>();
    }

    public Connection()
    {
       
    }

    public void Disconnect()
    {
        socket.Shutdown(SocketShutdown.Both);
        socket.Disconnect(false);
    }
    public void QueuePacket(BasePacket packet)
    {
        sendPacketQueue.Enqueue(packet);
    }

    public byte[] GetNextPacketInQueue()
    {
        if (!SocketConnected(socket))
        {
            return null;
        }


        while (sendPacketQueue.Count > 0)
        {
            BasePacket packet = sendPacketQueue.Dequeue();
            byte[] packetBytes = packet.GetPacketBytes();
            byte[] buffer = new byte[0xffff];
            Array.Copy(packetBytes, buffer, packetBytes.Length);
            return buffer;
        }
        throw new Exception("Something happened in getting queued packet");

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

    public void EstablishConnection()
    {

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            IPAddress[] ip = Dns.GetHostAddresses("127.0.0.1");


            StatusBoxHandler.statusText = "Connecting...";

            IPEndPoint remoteEP = new IPEndPoint(ip[0], 3425);
         //   socket.Connect(remoteEP, new AsyncCallback(ConnectCallBack), socket);
            socket.Connect(remoteEP);

            StatusBoxHandler.statusText = "Established Connection";

        }
        catch (Exception e)
        {
            StatusBoxHandler.readyToClose = true;
            StatusBoxHandler.statusText = e.Message;
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
               CursorInput.menuHandler.SetDestroyStatusBox();
               CursorInput.menuHandler.SetStatusText(e.Message);
               Debug.Log(e);
               clientConnection.socket.Shutdown(SocketShutdown.Both);
               clientConnection.socket.Close();
           }


       }*/

    public void Send(BasePacket packetToSend)
    {
        if (socket == null)
        {
            Debug.Log("null as bro");
        }

        socket.BeginSend(packetToSend.GetPacketBytes(), 0, packetToSend.GetPacketBytes().Length, 0,
            new AsyncCallback(SendCallBack), socket);
    }

    private void SendCallBack(IAsyncResult aSyncResult)
    {
        socket = (Socket)aSyncResult.AsyncState;
        socket.EndSend(aSyncResult);

        socket.BeginReceive(buffer, 0, buffer.Length, 0,
    new AsyncCallback(ReceiveCallBack), socket);
    }




    private void ReceiveCallBack(IAsyncResult aSyncResult)
    {
        try
        {
            int bytesRead = socket.EndReceive(aSyncResult);


            if (bytesRead > 0)
            {
                int offset = 0;

                //build/compile packets until can no longer or data is finished
                while (true)
                {
                    BasePacket basePacket = BuildPacket(ref offset, buffer, bytesRead);
                    if (basePacket == null)
                    {
                        break;
                    }
                    else
                    {

                        packetProcessor.ProcessPacket(basePacket);
                    }

                }
                //Not all bytes consumed, transfer leftover to beginning
                if (offset < bytesRead)
                {
                    Array.Copy(buffer, offset, buffer, 0, bytesRead - offset);
                }

                Array.Clear(buffer, bytesRead - offset, buffer.Length - (bytesRead - offset));

                if (offset < bytesRead)
                //need offset since not all bytes consumed
                {
                    socket.BeginReceive(buffer, bytesRead - offset, buffer.Length - (bytesRead - offset), SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
                }
                else
                {
                    socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
                }
            }
            else
            {
                Debug.Log("Lost connection to server");
            }



        }
        catch (Exception e) {
            Debug.Log("something went wrong");
            Debug.Log(e);
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
        //might be able to remove all these readytoclose checks in statusboxhandler and in here
        StatusBoxHandler.readyToClose = true;
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }


}
