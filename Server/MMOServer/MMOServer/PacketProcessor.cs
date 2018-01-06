using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MMOServer
{
    class PacketProcessor
    {
        //basically see what kind of packet it is and decide what to do with it
        private AccountPacket ap;
        private ClientConnection client;
        private const int MAX_AMOUNT_OF_CHARACTERS_ALLOWED = 3;

        public void ProcessPacket(ClientConnection client, BasePacket packet)
        {
            this.client = client;

            //        BasePacket.DecryptPacket(client.blowfish, ref packet);

            //else
            packet.debugPrintPacket();
            List<SubPacket> subPackets = packet.GetSubpackets();
            foreach (SubPacket subPacket in subPackets)
            {
                subPacket.debugPrintSubPacket();

                if (subPacket.header.type == (ushort)SubPacketTypes.Account)
                {
                    ProcessAccountPacket(client, subPacket);
                }

                if (subPacket.header.type == (ushort)SubPacketTypes.GamePacket)
                {
                    switch (subPacket.gameMessage.opcode)
                    {
                        case (ushort)GamePacketOpCode.CreateCharacter:
                            if (CheckCharacterCreatePacket(subPacket))
                            {
                                PerformCharacterCreate(subPacket);
                            }
                            break;

                        case (ushort)GamePacketOpCode.CharacterListQuery:
                            ProcessCharacterListQueryPacket(subPacket);
                            break;

                        case (ushort)GamePacketOpCode.CharacterDeleteQuery:
                            ProcessCharacterDeleteRequest(subPacket);
                            break;

                        case (ushort)GamePacketOpCode.Handshake:
                            ProcessHandshake(subPacket);
                            break;
                    }
                }

            }
        }

        //client in this case will be WorldServer
        private void ProcessHandshake(SubPacket receivedPacket)
        {
            //search connected clients for address 
            //send characterid and address from receivedPacket back to worldserver
            HandshakePacket received = new HandshakePacket(receivedPacket.data);
            foreach (var connection in LoginServer.mConnectionList)
            {
                if (connection.GetIp() == received.ClientAddress && characterIdPresentInClient(received.CharacterId, connection))
                {
                    //TODO: Separate this into a method
                    AcknowledgePacket ack = new AcknowledgePacket(true, connection.GetIp(), (uint)received.CharacterId);
                    SubPacket sp = new SubPacket(GamePacketOpCode.Acknowledgement, 0, 0, ack.GetBytes(), SubPacketTypes.GamePacket);
                    BasePacket successPacketToSend = BasePacket.CreatePacket(sp, true, false);
                    AckResponseToWorldServer(successPacketToSend);
                    connection.Disconnect(); //drop the client connection
                    return;
                }
            }
            AcknowledgePacket ackFailure = new AcknowledgePacket(true, received.ClientAddress, (uint)received.CharacterId);
            SubPacket fail = new SubPacket(GamePacketOpCode.Acknowledgement, 0, 0, ackFailure.GetBytes(), SubPacketTypes.GamePacket);
            BasePacket failPacketToSend = BasePacket.CreatePacket(fail, true, false);
            AckResponseToWorldServer(failPacketToSend);
        }

        private void AckResponseToWorldServer(BasePacket packetToSend)
        {
            
            packetToSend.header.connectionType = (ushort)BasePacketConnectionTypes.Connect;
            IPAddress[] ip = Dns.GetHostAddresses("127.0.0.1");
            IPEndPoint remoteEP = new IPEndPoint(ip[0], 3435);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(remoteEP);
            ClientConnection temp = new ClientConnection();
            temp.socket = socket;
            temp.QueuePacket(packetToSend);
            temp.FlushQueuedSendPackets();
        }

        private bool characterIdPresentInClient(uint characterId, ClientConnection temp)
        {
            foreach (var id in temp.CharacterIds)
            {
                if (id == characterId)
                {
                    return true;
                }
            }
            return false;
        }

        private void ProcessCharacterDeleteRequest(SubPacket receivedPacket)
        {
            LoginDatabase db = new LoginDatabase();
            CharacterDeletePacket deletePacket = new CharacterDeletePacket(receivedPacket);
            int error = db.DeleteCharacterFromDb(deletePacket.CharId);

            if (error == -1)
            {
                SubPacket success = new SubPacket(GamePacketOpCode.CharacterDeleteSuccess, 0, 0, System.Text.Encoding.Unicode.GetBytes("Character deleted successfully"), SubPacketTypes.GamePacket);
                BasePacket basePacket = BasePacket.CreatePacket(success, client.authenticated, false);
                client.QueuePacket(basePacket);
            }
            else
            {
                //send error packet here
            }

        }

        private void ProcessCharacterListQueryPacket(SubPacket receivedPacket)
        {
            LoginDatabase db = new LoginDatabase();
            CharacterQueryPacket cq = new CharacterQueryPacket();
            string accountName = cq.ReadAccountName(receivedPacket);
            Console.WriteLine("account name for CL: " + accountName);
            var accountId = db.GetAccountIdFromAccountName(accountName);
            Console.WriteLine("ID for CL: " + accountId);
            var characterList = db.GetListOfCharacters(accountId);
            var packets = cq.BuildResponsePacket(characterList);
            Console.WriteLine("Character packeted authenticated = " + client.authenticated);
            BasePacket packetsToSend = BasePacket.CreatePacket(packets, client.authenticated, false);
            Console.WriteLine("---Character Query Packet---");
            for(var i = 0; i < characterList.Count; i++)
            {
                int characterId = int.Parse(characterList[i][0]);
                client.CharacterIds[i] = characterId;
            }
            packetsToSend.debugPrintPacket();
            client.QueuePacket(packetsToSend);
        }

        private void ProcessAccountPacket(ClientConnection client, SubPacket packet)
        {
            ap = new AccountPacket();
            ErrorPacket ep = new ErrorPacket();
            ap.Read(packet.GetAccountHeaderBytes(), packet.data);
            if (!ap.register)//if account is logging in
            {
                LoginDatabase db = new LoginDatabase();
                List<string> account = db.CheckUserInDb(ap.userName, ap.password);
                switch (account.Count)
                {
                    case 0:
                        var packetToSend = ep.buildPacket(GamePacketOpCode.AccountError, ErrorCodes.NoAccount, "Account does not exist");
                        Console.WriteLine("Attempted log in for username: {0} pw: {1}, account does not exist", ap.userName, ap.password);
                        QueueErrorPacket(packetToSend);
                        break;

                    case 1:
                        //password incorrect
                        packetToSend = ep.buildPacket(GamePacketOpCode.AccountError, ErrorCodes.WrongPassword, "Wrong username or password");
                        Console.WriteLine("Attempted log in for username: {0} pw: {1}, password incorrect", ap.userName, ap.password);
                        QueueErrorPacket(packetToSend);
                        break;

                    case 2:
                        //user and password found
                        Console.WriteLine("Username: {0} Password: {1} has logged in successfully", account[0], account[1]);
                        SubPacket success = new SubPacket(GamePacketOpCode.AccountSuccess, 0, 0, System.Text.Encoding.Unicode.GetBytes("Login Successful"), SubPacketTypes.GamePacket);
                        client.authenticated = true;
                        BasePacket basePacket = BasePacket.CreatePacket(success, client.authenticated, false);
                        client.QueuePacket(basePacket);
                        break;

                    default:
                        throw new Exception("somehow found more than 2 colums in DB");
                }
            }
            else //account is registering
            {
                LoginDatabase db = new LoginDatabase();
                var succeeded = db.AddUserToDb(ap.userName, ap.password);
                if (succeeded)
                {
                    Console.WriteLine("Username: {0} Password: {1} has been registered successfully", ap.userName, ap.password);
                    SubPacket success = new SubPacket(GamePacketOpCode.RegisterSuccess, 0, 0, System.Text.Encoding.Unicode.GetBytes("Registration Successful"), SubPacketTypes.GamePacket);
                    BasePacket basePacket = BasePacket.CreatePacket(success, false, false);
                    client.QueuePacket(basePacket);
                }
                else
                {
                    var packetToSend = ep.buildPacket(GamePacketOpCode.AccountError, ErrorCodes.DuplicateAccount, "Account already registered");
                    QueueErrorPacket(packetToSend);
                }


            }
        }
     
        private void QueueErrorPacket(SubPacket subPacket)
        {
            BasePacket errorBasePacket = BasePacket.CreatePacket(subPacket, client.authenticated, false);
            client.QueuePacket(errorBasePacket);
        }

        private bool CheckCharacterCreatePacket(SubPacket subPacket)
        {
            LoginDatabase db = new LoginDatabase();
            CharacterCreatePacket cp = new CharacterCreatePacket(subPacket.data);
            var summedStats = cp.GetStr() + cp.GetAgi() + cp.GetInt() + cp.GetVit() + cp.GetDex();
            Console.WriteLine(ap.userName);
            if (summedStats != cp.statsAllowed)
            {
                Console.WriteLine("summed stats: {0} was different from packet: {1}", summedStats, cp.statsAllowed);
                Console.WriteLine("Someone is trying to hack the client for stat character creation. Username was {0}", ap.userName);
                return false;
            }
            Console.WriteLine("Received new character creation request for character name: {0}", cp.GetCharacterName());
            if (db.GetNumberOfCharactersForAccount(ap.userName) <= MAX_AMOUNT_OF_CHARACTERS_ALLOWED)
            {
                if (db.EnsuredThatCharacterSlotIsUniqueValue(cp.selectedSlot, ap.userName))
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("Somone is trying to hack character slots. Username was {0}", ap.userName);
                    return false;
                }
            }
            return false;
        }

        private void PerformCharacterCreate(SubPacket subPacket)
        {
            LoginDatabase db = new LoginDatabase();
            CharacterCreatePacket cp = new CharacterCreatePacket(subPacket.data);
            try
            {
                db.AddCharacterToDb(ap.userName, cp);
                SubPacket success = new SubPacket(GamePacketOpCode.CreateCharacterSuccess, 0, 0, System.Text.Encoding.Unicode.GetBytes("Character created successfully"), SubPacketTypes.GamePacket);
                BasePacket basePacket = BasePacket.CreatePacket(success, client.authenticated, false);
                client.QueuePacket(basePacket);
            }
            catch (MySqlException e)
            {
                ErrorPacket ep = new ErrorPacket();
                SubPacket packetToSend;
                if (e.Number == (int)ErrorCodes.DuplicateCharacter)
                {
                    packetToSend = ep.buildPacket(GamePacketOpCode.CreateCharacterError, ErrorCodes.DuplicateCharacter, "Character with that name already exists");
                }
                else
                {
                    packetToSend = ep.buildPacket(GamePacketOpCode.CreateCharacterError, ErrorCodes.UnknownDatabaseError, "Unknown database error occurred");
                }

                QueueErrorPacket(packetToSend);
            }
         
        }
    }
}
