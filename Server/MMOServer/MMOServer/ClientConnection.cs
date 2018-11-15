using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using Collections.Generic;

namespace MMOServer
{
    public class ClientConnection
    {
        public Blowfish blowfish;
        public Socket socket;
        public bool authenticated;
        public byte[] buffer = new byte[0xffff];
        public CircularBuffer<byte> incomingStream = new CircularBuffer<byte>(1024);
        public BlockingCollection<BasePacket> sendPacketQueue = new BlockingCollection<BasePacket>(100);
        public int lastPartialSize = 0;
        private int accountId;
        private int[] characterId = new int[3];
        private IPAddress clientIpAddress;
        private int clientPort;
        public PacketProcessor PacketProcessor{get;set;}
        public string fullAddress { get; set; }

        public IPAddress ClientIpAddress
        {
            get
            {
                return clientIpAddress;
            }

            set
            {
                clientIpAddress = value;
            }
        }

        public int ClientPort
        {
            get
            {
                return clientPort;
            }

            set
            {
                clientPort = value;
            }
        }

        /// <summary>
        /// Gets/sets character id array that is associated with this connection
        /// </summary>
        public int[] CharacterIds
        {
            get
            {
                return characterId;
            }

            set
            {
                characterId = value;
            }
        }

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

        public string GetFullAddress()
        {
            return fullAddress;
        }

        public string GetIp()
        {
            return (socket.RemoteEndPoint as IPEndPoint).Address + "";
        }

        public int GetPort()
        {
            return (socket.RemoteEndPoint as IPEndPoint).Port;
        }

        public void Disconnect()
        {
            authenticated = false;
            socket.Shutdown(SocketShutdown.Both);
            socket.Disconnect(false);
        }

        public int GetAccountId()
        {
            return accountId;
        }

        public void SetAccountId(int accountId)
        {
            this.accountId = accountId;
        }

    }
}
