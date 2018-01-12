using MMOServer;
using MMOWorldServer.Actors;
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

            if (packet.header.connectionType == (ushort)BasePacketConnectionTypes.Connect)
            {
                ProcessConnectPackets(subPackets);
            }
            if (packet.header.connectionType == (ushort)BasePacketConnectionTypes.Zone)
            {
                processZonePackets(subPackets);
            }
        }

        private void processZonePackets(List<SubPacket> subPackets)
        {
            foreach (SubPacket subPacket in subPackets)
            {
                if (subPacket.gameMessage.opcode != (ushort)GamePacketOpCode.PositionPacket)
                {
                    subPacket.debugPrintSubPacket();
                }


                switch (subPacket.gameMessage.opcode)
                {
                    case ((ushort)GamePacketOpCode.PositionPacket):
                        HandlePositionPacket(subPacket);
                        break;

                    case ((ushort)GamePacketOpCode.NearbyActorsQuery):
                        HandleNearbyActorsQuery(subPacket);
                        break;
                }
            }
        }

        private void HandleNearbyActorsQuery(SubPacket subPacket)
        {
            PositionsInBoundsPacket posInBoundsPacket = new PositionsInBoundsPacket(subPacket.data);
            if (WorldServer.mConnectedPlayerList.TryGetValue(subPacket.header.sourceId, out Character character))
            {
                character.SetCharacterCameraBounds(posInBoundsPacket.XMin, posInBoundsPacket.XMax, posInBoundsPacket.YMin, posInBoundsPacket.YMax);
                List<SubPacket> nearbyCharacters = new List<SubPacket>();
                bool foundNearby = false;
                foreach (KeyValuePair<uint, Character> entry in WorldServer.mConnectedPlayerList)
                {
                    if ((entry.Key != subPacket.header.sourceId) && entry.Value.XPos > character.BoundsXMin &&
                        entry.Value.XPos < character.BoundsXMax && entry.Value.YPos > character.BoundsYMin &&
                        entry.Value.YPos < character.BoundsYMax)
                    {
                        Console.WriteLine("FOUND NEARBY PLAYER!");
                        foundNearby = true;
                        PositionPacket packet = new PositionPacket(entry.Value.XPos, entry.Value.YPos, true, entry.Value.CharacterId);
                        SubPacket sp = new SubPacket(GamePacketOpCode.NearbyActorsQuery, entry.Value.CharacterId, 0, packet.GetBytes(), SubPacketTypes.GamePacket);
                        nearbyCharacters.Add(sp);
                    }
                }
                if (foundNearby)
                {
                    client.QueuePacket(BasePacket.CreatePacket(nearbyCharacters, true, false));
                    client.FlushQueuedSendPackets();
                }                
            }
            else
            {
                //DC player?
            }

        }

        private void HandlePositionPacket(SubPacket subPacket)
        {
            PositionPacket packet = new PositionPacket(subPacket.data);
            WorldServer.mConnectedPlayerList.TryGetValue(subPacket.header.sourceId, out Character character);
            character.SavePositions(packet.XPos, packet.YPos);
            //Console.WriteLine("Character position: " + character.XPos + "," + character.YPos);
        }

        private void ProcessConnectPackets(List<SubPacket> subPackets)
        {
            foreach (SubPacket subPacket in subPackets)
            {
                subPacket.debugPrintSubPacket();
                switch (subPacket.gameMessage.opcode)
                {
                    case ((ushort)GamePacketOpCode.Handshake):
                        try
                        {
                            Console.WriteLine("100% CORRECT CLIENT CONNECTION: " + client.GetFullAddress());
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
                            if (WorldServer.mConnectedPlayerList.TryGetValue(ack.CharacterId, out Character character))
                            {
                                client.Disconnect(); //this is 100% login server connection, don't doubt this
                                client = character.WorldClientConnection;
                                Console.WriteLine("Client looks legit: " + (ack.ClientAddress == client.GetIp()));    
                                
                                WorldDatabase.AddToOnlinePlayerList(character.CharacterId, ack.ClientAddress);
                                client.SessionId = WorldDatabase.GetSessionId(character.CharacterId);
                                Console.WriteLine("Sending ack received from login server back to: " + client.GetFullAddress());
                                AcknowledgePacket responseAck = new AcknowledgePacket(ack.AckSuccessful, client.SessionId);
                                SubPacket sp = new SubPacket(GamePacketOpCode.Acknowledgement, 0, 0, responseAck.GetResponseFromWorldServerBytes(), SubPacketTypes.GamePacket);
                                client.QueuePacket(BasePacket.CreatePacket(sp, true, false));
                                client.FlushQueuedSendPackets();
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Client has connected but is not in Connected Player List.. Not sure what to do here");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Ack not successful, removing from connected player list");
                            WorldServer.mConnectedPlayerList.Remove(ack.CharacterId);
                            client.Disconnect();
                        }
                        break;

                    case (((ushort)GamePacketOpCode.Disconnect)):
                        DisconnectPacket dc = new DisconnectPacket(subPacket.data);
                        Console.WriteLine("Got DC packet");
                        WorldDatabase.RemoveFromOnlinePlayerList(dc.CharacterId);
                        WorldServer.mConnectedPlayerList.Remove(dc.CharacterId);
                        client.Disconnect();
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
            Console.WriteLine("Confirming client connection with login server");
            IPAddress[] ip = Dns.GetHostAddresses(LOGIN_SERVER_IP);
            Character character = new Character(BitConverter.ToUInt32(subPacket.data, 0));
            client.HasHandshakedWorldServerToClient = true;
            character.WorldClientConnection = client;

            if (!WorldServer.mConnectedPlayerList.ContainsKey(character.CharacterId))
            {
                WorldServer.mConnectedPlayerList.Add(character.CharacterId, character);
            }
            else
            {
                Console.WriteLine("WARNING! : Connected player already exists and trying to add them into list");
            }

            IPEndPoint remoteEP = new IPEndPoint(ip[0], LOGIN_SERVER_PORT);
            socket.Connect(remoteEP);
            HandshakePacket packet = new HandshakePacket(client.GetIp(), client.GetPort(), character.CharacterId);
            Console.WriteLine("PORT FROM CLIENT:" + client.GetPort());
            Console.WriteLine("IP FROM CLIENT:" + client.GetIp());
            Console.WriteLine("CHARACTER ID FROM CLIENT: " + character.CharacterId);
            SubPacket sp = new SubPacket(GamePacketOpCode.Handshake, 0, 0, packet.GetBytes(), SubPacketTypes.GamePacket);
            BasePacket packetToSend = BasePacket.CreatePacket(sp, true, false);

            //send packet to login server for confirmation
            WorldClientConnection connectionToLoginServer = new WorldClientConnection();
            connectionToLoginServer.socket = socket;
            connectionToLoginServer.QueuePacket(packetToSend);
            connectionToLoginServer.FlushQueuedSendPackets();
            connectionToLoginServer.Disconnect();
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
