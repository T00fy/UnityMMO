using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using MMOServer;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using SharpConfig;
using System.IO;

public class Connection : MonoBehaviour
{
    private Socket socket;
    [HideInInspector]
    public const int BUFFER_SIZE = 65535;
    [HideInInspector]
    public byte[] buffer = new byte[0xffff];
    //       public CircularBuffer<byte> incomingStream = new CircularBuffer<byte>(1024);
    public List<SubPacket> sendPacketQueue = new List<SubPacket>(100);
    [HideInInspector]
    public int lastPartialSize = 0;
    private Processor packetProcessor;
    private bool disconnecting;

    void Start()
    {
        packetProcessor = GameObject.FindGameObjectWithTag("PacketProcessor").GetComponent<PacketProcessor>();
        Configuration cfg = new Configuration();
        if (!File.Exists("config.cfg"))
        {
            cfg["Connection"]["Login"].StringValue = Data.LOGIN_ADDRESS;
            cfg["Connection"]["World"].StringValue = Data.WORLD_ADDRESS;
            cfg.SaveToFile("config.cfg");
        }
        else
        {
            cfg = Configuration.LoadFromFile("config.cfg");
            var section = cfg["Connection"];

            Data.LOGIN_ADDRESS = section["Login"].StringValue;
            Data.WORLD_ADDRESS = section["World"].StringValue;
        }

        
    }

    public Connection()
    {

    }

    public void Disconnect()
    {
        socket.Shutdown(SocketShutdown.Both);
        socket.Disconnect(false);
        
    }
    public void QueuePacket(SubPacket packet)
    {
        sendPacketQueue.Add(packet);
    }

    public List<SubPacket> GetQueue()
    {
        return sendPacketQueue;
    }

    public Socket GetSocket()
    {
        return socket;
    }

    public bool SocketConnected()
    {
        bool part1 = socket.Poll(1000, SelectMode.SelectRead);
        bool part2 = (socket.Available == 0);
        if ((part1 && part2) || !socket.Connected)
            return false;
        else
            return true;
    }

    public void EstablishConnection(string ipAddress, int port)
    {

        socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            IPAddress[] ip = Dns.GetHostAddresses(ipAddress);


            StatusBoxHandler.statusText = "Connecting...";

            IPEndPoint remoteEP = new IPEndPoint(ip[0], port);
            Debug.Log(ip[0]);
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

    public void Send(BasePacket packetToSend)
    {
        try
        {
            if (socket == null)
            {
                Debug.Log("sockt is null/missing");
            }

            socket.BeginSend(packetToSend.GetPacketBytes(), 0, packetToSend.GetPacketBytes().Length, 0,
                new AsyncCallback(SendCallBack), socket);
        }
        catch
        {
            Debug.Log("should boot back to login menu");
            //boot back to login menu
        }

    }

    private void SendCallBack(IAsyncResult aSyncResult)
    {
        socket = (Socket)aSyncResult.AsyncState;
        socket.EndSend(aSyncResult);

        socket.BeginReceive(buffer, 0, buffer.Length, 0,
    new AsyncCallback(ReceiveCallBack), socket);
    }


    public void FlushQueuedSendPackets(BasePacketConnectionTypes header = BasePacketConnectionTypes.Zone)
    {
        if (!socket.Connected)
            return;

        while (sendPacketQueue.Count > 0)
        {
            BasePacket packet = BasePacket.CreatePacket(sendPacketQueue, PacketProcessor.isAuthenticated, false);
            packet.header.connectionType = (ushort)header;

            try
            {
                socket.Send(packet.GetPacketBytes());
                sendPacketQueue.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine("Weird case, socket was d/ced: {0}", e);
            }
        }
    }

    public void SetPacketProcessor(Processor processor)
    {
        packetProcessor = processor;
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

                //Build any queued subpackets into basepackets and send
                FlushQueuedSendPackets();

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
        }
        catch (Exception e)
        {
            Debug.Log("something went wrong ");
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

    void OnApplicationQuit()
    {

        SendDisconnectPacket();
    }

    public void SendDisconnectPacket()
    {
        if (Data.CHARACTER_ID != 0)
        {
            DisconnectPacket dcPacket = new DisconnectPacket(Data.CHARACTER_ID);
            SubPacket packet = new SubPacket(GamePacketOpCode.Disconnect, Data.CHARACTER_ID, 0, dcPacket.GetBytes(), SubPacketTypes.GamePacket);
            var packetToSend = BasePacket.CreatePacket(packet, PacketProcessor.isAuthenticated, false);
            packetToSend.header.connectionType = (ushort)BasePacketConnectionTypes.Connect;
            socket.Send(packetToSend.GetPacketBytes());
        }
    }
}
