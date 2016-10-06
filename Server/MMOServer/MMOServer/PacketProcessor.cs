using System;
using System.Collections.Generic;

namespace MMOServer
{
    class PacketProcessor
    {
        //basically see what kind of packet it is and decide what to do with it


        public void ProcessPacket(ClientConnection client, BasePacket packet)
        {

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
                            CheckCharacterCreatePacket(subPacket);
                            break;
                    }
                }

            }
        }

        private void ProcessAccountPacket(ClientConnection client, SubPacket packet)
        {
            AccountPacket ap = new AccountPacket();
            ErrorPacket ep = new ErrorPacket();
            ap.Read(packet.GetAccountHeaderBytes(), packet.data);
            if (!ap.register)//if account is logging in
            {
                Database db = new Database();
                List<string> account = db.CheckUserInDb(ap.userName, ap.password);
                switch (account.Count)
                {
                    case 0:
                        var packetToSend = ep.buildPacket(ErrorCodes.NoAccount, "Account does not exist");
                        QueueErrorPacket(packetToSend, client);
                        break;

                    case 1:
                        //password incorrect
                        packetToSend = ep.buildPacket(ErrorCodes.WrongPassword, "Wrong username or password");
                        QueueErrorPacket(packetToSend, client);
                        break;

                    case 2:
                        //user and password found
                        Console.WriteLine("Username: {0} Password: {1} has logged in successfully", account[0], account[1]);
                        SubPacket success = new SubPacket(GamePacketOpCode.AccountSuccess, 0, 0, System.Text.Encoding.Unicode.GetBytes("Login Successful"), SubPacketTypes.GamePacket);
                        BasePacket basePacket = BasePacket.CreatePacket(success, true, false);
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
                    BasePacket basePacket = BasePacket.CreatePacket(success, true, false);
                    client.QueuePacket(basePacket);
                }
                else
                {
                    var packetToSend = ep.buildPacket(ErrorCodes.DuplicateAccount, "Account already registered");
                    QueueErrorPacket(packetToSend, client);
                }


            }
        }
     
        private void QueueErrorPacket(SubPacket subPacket, ClientConnection client)
        {
            BasePacket errorBasePacket = BasePacket.CreatePacket(subPacket, false, false);
            client.QueuePacket(errorBasePacket);
        }

        private void CheckCharacterCreatePacket(SubPacket subPacket)
        {
            Console.WriteLine("Received character packet");
            CharacterPacket cp = new CharacterPacket(subPacket.data);
            Console.WriteLine("Received: " + cp.GetCharacterName() + cp.GetDex());
            
        }



    }
}
