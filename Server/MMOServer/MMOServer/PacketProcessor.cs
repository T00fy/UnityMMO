using System;
using System.Collections.Generic;

namespace MMOServer
{
    class PacketProcessor
    {
        //basically see what kind of packet it is and decide what to do with it
        private AccountPacket ap;
        private ClientConnection client;

        public void ProcessPacket(ClientConnection client, BasePacket packet)
        {
            this.client = client;

            //        BasePacket.DecryptPacket(client.blowfish, ref packet);

            //else
            packet.debugPrintPacket();
            List<SubPacket> subPackets = packet.GetSubpackets();
            foreach (SubPacket subPacket in subPackets)
            {
                Console.WriteLine("OPCODE: "+ subPacket.gameMessage.opcode);
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
                            CheckCharacterCreatePacket(subPacket);
                            break;
                    }
                }
                if (subPacket.header.type == (ushort)SubPacketTypes.GamePacket)
                {
                    switch (subPacket.gameMessage.opcode)
                    {
                        case (ushort)GamePacketOpCode.CharacterListQuery:
                            ProcessCharacterListQueryPacket(subPacket);
                            break;
                    }
                }

            }
        }

        private void ProcessCharacterListQueryPacket(SubPacket receivedPacket)
        {
            Database db = new Database();
            CharacterQueryPacket cq = new CharacterQueryPacket();
            string accountName = cq.ReadAccountName(receivedPacket);
            var characterList = db.GetListOfCharacters(accountName);
            var packets = cq.BuildResponsePacket(characterList);
            Console.WriteLine("Character packeted authenticated = " + client.authenticated);
            BasePacket packetsToSend = BasePacket.CreatePacket(packets, client.authenticated, false);

            client.QueuePacket(packetsToSend);
        }

        private void ProcessAccountPacket(ClientConnection client, SubPacket packet)
        {
            ap = new AccountPacket();
            ErrorPacket ep = new ErrorPacket();
            ap.Read(packet.GetAccountHeaderBytes(), packet.data);
            if (!ap.register)//if account is logging in
            {
                Database db = new Database();
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
                Database db = new Database();
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

        private void CheckCharacterCreatePacket(SubPacket subPacket)
        {
            Database db = new Database();
            CharacterCreatePacket cp = new CharacterCreatePacket(subPacket.data);
            var summedStats = cp.GetStr() + cp.GetAgi() + cp.GetInt() + cp.GetVit() + cp.GetDex();
            Console.WriteLine(ap.userName);
            if (summedStats != cp.statsAllowed)
            {
                Console.WriteLine("summed stats: {0} was different from packet: {1}", summedStats, cp.statsAllowed);
                Console.WriteLine("shouldn't get here unless someone is trying to hack the client. Username was {0}", ap.userName);
            }
            Console.WriteLine("Received new character creation request for character name: {0}", cp.GetCharacterName());
            int error = db.AddCharacterToDb(ap.userName, cp);
            if (error == -1)
            {
                SubPacket success = new SubPacket(GamePacketOpCode.CreateCharacterSuccess, 0, 0, System.Text.Encoding.Unicode.GetBytes("Character created successfully"), SubPacketTypes.GamePacket);
                BasePacket basePacket = BasePacket.CreatePacket(success, client.authenticated, false);
                client.QueuePacket(basePacket);
                //created successfully
            }
            else
            {
                ErrorPacket ep = new ErrorPacket();
                if (error == (int)ErrorCodes.DuplicateCharacter)
                {
                    var packetToSend = ep.buildPacket(GamePacketOpCode.CreateCharacterError, ErrorCodes.DuplicateCharacter, "Character with that name already exists");
                }
                if (error == (int)ErrorCodes.UnknownDatabaseError)
                {
                    var packetToSend = ep.buildPacket(GamePacketOpCode.CreateCharacterError, ErrorCodes.UnknownDatabaseError, "Unknown database error occurred");
                    QueueErrorPacket(packetToSend);
                }
            }

        }



    }
}
