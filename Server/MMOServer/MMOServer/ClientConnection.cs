using System;
using System.Collections.Concurrent;
using Cyotek.Collections.Generic;
using System.Net.Sockets;
using System.Net;

namespace MMOServer
{
    class ClientConnection
    {
        public Blowfish blowfish;
        public Socket socket;
        public byte[] buffer = new byte[0xffff];
        public CircularBuffer<byte> incomingStream = new CircularBuffer<byte>(1024);
        public BlockingCollection<BasePacket> sendPacketQueue = new BlockingCollection<BasePacket>(100);
        public int lastPartialSize = 0;



  /*      public void ProcessIncoming(int bytesIn)
        {
            if (bytesIn == 0)
                return;

            incomingStream.Put(buffer, 0, bytesIn);
        }*/

        public void QueuePacket(BasePacket packet)
        {
            sendPacketQueue.Add(packet);
        }

        public void FlushQueuedSendPackets()
        {
            if (!socket.Connected)
                return;

            while (sendPacketQueue.Count > 0)
            {
                BasePacket packet = sendPacketQueue.Take();
                byte[] packetBytes = packet.GetPacketBytes();
                byte[] buffer = new byte[0xffff];
                Array.Copy(packetBytes, buffer, packetBytes.Length);
                try
                {
                    socket.Send(packetBytes);
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Format("Weird case, socket was d/ced: {0}", e));
                }
            }
        }

        public string GetAddress()
        {
            return string.Format("{0}:{1}", (socket.RemoteEndPoint as IPEndPoint).Address, (socket.RemoteEndPoint as IPEndPoint).Port);
        }

        public void Disconnect()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Disconnect(false);
        }

    }
}
