using MMOServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MMOWorldServer
{
    /// <summary>
    /// 
    /// </summary>
    class WorldPacketProcessor
    {
        private WorldClientConnection client;
        //List<ClientConnection> mConnections;
        private const string LOGIN_SERVER_IP = "127.0.0.1";
        private const int LOGIN_SERVER_PORT = 3425;


        public void ProcessPacket(WorldClientConnection client, BasePacket packet)
        {
            this.client = client;
            packet.debugPrintPacket();
            List<SubPacket> subPackets = packet.GetSubpackets();
            /*            if (packet.header.connectionType == (ushort)BasePacketConnectionTypes.Zone)
                        {
                            ProcessZonePackets(subPackets);
                            //do zone stuff here
                        }
                        if (packet.header.connectionType == (ushort)BasePacketConnectionTypes.Chat)
                        {
                            ProcessChatPackets(subPackets);
                            //do chat stuff here
                        }*/

            if (packet.header.connectionType == (ushort)BasePacketConnectionTypes.Generic)
            {
                ProcessGenericPackets(subPackets);
            }
        }

        private void ProcessGenericPackets(List<SubPacket> subPackets)
        {
            foreach (SubPacket subPacket in subPackets)
            {
                subPacket.debugPrintSubPacket();
                Console.WriteLine(subPacket.gameMessage.opcode);
                switch (subPacket.gameMessage.opcode)
                {
                    case ((ushort)GamePacketOpCode.Handshake):
                        try
                        {
                            ConfirmClientConnectionWithLoginServer(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), subPacket);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        break;


                    case (((ushort)GamePacketOpCode.Acknowledgement)):
                        
                        AcknowledgePacket ack = new AcknowledgePacket(subPacket.data);
                        if (ack.AckSuccessful)
                        {
                            
                            foreach (var mClient in WorldServer.GetClientConnections()) //check this if any performance issues
                            {
                                Console.WriteLine(ack.CharacterId);
                                if (mClient.CharacterId == ack.CharacterId && mClient.HasHandshakedWorldServerToClient)//this is getting the wrong client
                                {           //maybe set a boolean in clientconnection that tells whether or not client is created from a server to server communication
                                    ConnectedPlayer connectedPlayer = new ConnectedPlayer(ack.CharacterId);
                                    connectedPlayer.ClientAddress = ack.ClientAddress;
                                    WorldServer.mConnectedPlayerList.Add(connectedPlayer.actorId, connectedPlayer);
                                    client = mClient;
                                    SubPacket sp = new SubPacket(GamePacketOpCode.Acknowledgement, 0, 0, subPacket.data, SubPacketTypes.GamePacket);
                                    client.QueuePacket(BasePacket.CreatePacket(sp, true, false));
                                    client.FlushQueuedSendPackets();
                                    Console.WriteLine("Sending ack back to: " + client.GetFullAddress());
                                    break;
                                }
                            }


                        }
                        break;



                    //if everything okay 
                    default:
                        break;
                }
            }


        }

        /// <summary>
        /// contact login server and ensure that the ipaddress of player is currently connected to it
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="subPacket"></param>
        private void ConfirmClientConnectionWithLoginServer(Socket socket, SubPacket subPacket)
        {
            IPAddress[] ip = Dns.GetHostAddresses(LOGIN_SERVER_IP);
            int characterId = BitConverter.ToInt32(subPacket.data, 0);
            client.CharacterId = characterId;
            client.HasHandshakedWorldServerToClient = true;
            IPEndPoint remoteEP = new IPEndPoint(ip[0], LOGIN_SERVER_PORT);
            socket.Connect(remoteEP);
            HandshakePacket packet = new HandshakePacket(client.GetIp(), client.GetPort(), characterId);
            //Console.WriteLine("PORT FROM CLIENT:" + client.GetPort());
            //Console.WriteLine("IP FROM CLIENT:" + client.GetIp());
            //Console.WriteLine("CHARACTER ID FROM CLIENT: " + client.CharacterId);
            SubPacket sp = new SubPacket(GamePacketOpCode.Handshake, 0, 0, packet.GetBytes(), SubPacketTypes.GamePacket);
            BasePacket packetToSend = BasePacket.CreatePacket(sp, true, false);
            WorldClientConnection server = new WorldClientConnection();
            server.socket = socket;
            server.QueuePacket(packetToSend);
            server.FlushQueuedSendPackets();
        }

        /*     private void ProcessChatPackets(List<SubPacket> subPackets)
             {
                 throw new NotImplementedException();
             }

             private void ProcessZonePackets(List<SubPacket> subPackets)
             {
                 throw new NotImplementedException();
             }*/
    }


}
