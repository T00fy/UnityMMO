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

public class Connection : MonoBehaviour
{
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
    private bool disconnecting;

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

    public Socket GetSocket()
    {
        return socket;
    }

    public byte[] GetNextPacketInQueue()
    {
        if (!SocketConnected())
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

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            IPAddress[] ip = Dns.GetHostAddresses(ipAddress);


            StatusBoxHandler.statusText = "Connecting...";

            IPEndPoint remoteEP = new IPEndPoint(ip[0], port);
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
                if (socket.RemoteEndPoint.ToString() != Data.LOGIN_IP)
                {
                    Debug.Log("Disconnected from world server");
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
        if (Data.SESSION_ID != 0)
        {
            DisconnectPacket dcPacket = new DisconnectPacket(Data.SESSION_ID);
            SubPacket packet = new SubPacket(GamePacketOpCode.Disconnect, 0, 0, dcPacket.GetBytes(), SubPacketTypes.GamePacket);
            var packetToSend = BasePacket.CreatePacket(packet, PacketProcessor.isAuthenticated, false);
            packetToSend.header.connectionType = (ushort)BasePacketConnectionTypes.Connect;
            socket.Send(packetToSend.GetPacketBytes());
        }
    }
}
