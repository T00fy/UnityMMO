using MMOServer;
using MMOWorldServer.Actors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MMOWorldServer
{
    class WorldClientConnection
    {
        //Connection stuff
        public Socket socket;
        public byte[] buffer;
        private BlockingCollection<BasePacket> SendPacketQueue = new BlockingCollection<BasePacket>(1000);
        public int lastPartialSize = 0;
        private bool worldServerToClient = false;

        //Instance Stuff
        public uint owner = 0;
        public int connType = 0;
        private IPAddress clientIpAddress;
        private int clientPort;
        
        public uint SessionId
        {
            get;

            set;
        }

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
        /// 
        /// </summary>
        public bool HasHandshakedWorldServerToClient
        {
            get
            {
                return worldServerToClient;
            }

            set
            {
                worldServerToClient = value;
            }
        }

        public void QueuePacket(BasePacket packet)
        {
            SendPacketQueue.Add(packet);
        }

        public void QueuePacket(SubPacket subpacket, bool isAuthed, bool isEncrypted)
        {
            SendPacketQueue.Add(BasePacket.CreatePacket(subpacket, isAuthed, isEncrypted));
        }

        public void FlushQueuedSendPackets()
        {
            if (!socket.Connected)
                return;

            while (SendPacketQueue.Count > 0)
            {
                BasePacket packet = SendPacketQueue.Take();

                byte[] packetBytes = packet.GetPacketBytes();

                try
                {
                    socket.Send(packetBytes);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Weird case, socket was d/ced: {0}", e);
                }
            }
        }

        public string GetFullAddress()
        {
            return string.Format("{0}:{1}", (socket.RemoteEndPoint as IPEndPoint).Address, (socket.RemoteEndPoint as IPEndPoint).Port);
        }

        public string GetIp()
        {
            return (socket.RemoteEndPoint as IPEndPoint).Address + "";
        }

        public int GetPort()
        {
            return (socket.RemoteEndPoint as IPEndPoint).Port;
        }

        public bool IsConnected()
        {
            return (socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
        }

        public void Disconnect()
        {
            if (socket.Connected)
                socket.Disconnect(false);
        }

        public void Ping()
        {
           // lastPingPacket = Utils.UnixTimeStampUTC();
        }

        public bool CheckIfDCing()
        {
            throw new NotImplementedException();
            /*    uint currentTime = Utils.UnixTimeStampUTC();
                if (currentTime - lastPingPacket >= 5000) //Show D/C flag
                    playerActor.SetDCFlag(true);
                else if (currentTime - lastPingPacket >= 30000) //DCed
                    return true;
                else
                    playerActor.SetDCFlag(false);
                return false;*/
        }

        public void UpdatePlayerActorPosition(float x, float y, ushort moveState)
        {
            throw new NotImplementedException();
            /*   playerActor.oldPositionX = playerActor.positionX;
               playerActor.oldPositionY = playerActor.positionY;
               playerActor.oldPositionZ = playerActor.positionZ;
               playerActor.oldRotation = playerActor.rotation;

               playerActor.positionX = x;
               playerActor.positionY = y;
               playerActor.moveState = moveState;

               GetActor().zone.UpdateActorPosition(GetActor());*/

        }
    }
}