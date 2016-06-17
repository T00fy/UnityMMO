using UnityEngine;
using System;
using MMOServer;
//using Cyotek.Collections.Generic;
using System.Net.Sockets;
using System.Collections.Generic;

public class ClientConnect {
    public Socket socket;
    public byte[] buffer = new byte[0xffff];
    public bool isAuthenticated;
//       public CircularBuffer<byte> incomingStream = new CircularBuffer<byte>(1024);
    public Queue<BasePacket> sendPacketQueue = new Queue<BasePacket>(100);
    public int lastPartialSize = 0;

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
}
