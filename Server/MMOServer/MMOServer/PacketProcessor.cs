using System;
using System.Collections.Generic;

namespace MMOServer
{
    class PacketProcessor
    {
        //basically see what kind of packet it is and decide what to do with it


        public void ProcessPacket(ClientConnection client, BasePacket packet)
        {

            BasePacket.DecryptPacket(client.blowfish, ref packet);

            //else
            packet.debugPrintPacket();
            Console.WriteLine("gets here");
            List<SubPacket> subPackets = packet.GetSubpackets();
            foreach (SubPacket subPacket in subPackets)
            {
                subPacket.debugPrintSubPacket();

                if (subPacket.header.type == (ushort)SubPacketTypes.Account)
                {
                    ProcessAccountPacket(client, subPacket);
                }

            }
        }

        private void ProcessAccountPacket(ClientConnection client, SubPacket packet)
        {
            AccountPacket ap = new AccountPacket();
            ap.Read(packet.getHeaderBytes(), packet.data);
            if (!ap.register)//if account is logging in
            {
                Database db = new Database();
                List<string> account = db.CheckUserInDb(ap.userName, ap.passWord);
                switch (account.Count)
                {
                    case 0:
                        ErrorPacket errorPacket = new ErrorPacket(ErrorCodes.NoAccount, "Account does not exist");
                        QueueErrorPacket(errorPacket, client);
                        break;

                    case 1:
                        //password incorrect
                        ErrorPacket errorPacket2 = new ErrorPacket(ErrorCodes.WrongPassword, "Wrong username or password");
                        QueueErrorPacket(errorPacket2, client);
                        break;

                    case 2:
                        //user and password found
                        Console.WriteLine("Username: {0} Password: {1} has logged in successfully", account[1], account[2]);
                        SubPacket success = new SubPacket(GamePacketOpCode.Success, 0, 0, System.Text.Encoding.Unicode.GetBytes("Login Successful"), SubPacketTypes.GamePacket);
                        BasePacket basePacket = BasePacket.CreatePacket(success, true, false);
                        client.QueuePacket(basePacket);
                        break;

                    default:
                        throw new Exception("somehow found more than 2 colums in DB");
                }
            }
            else
            {
                Database db = new Database();
                var succeeded = db.AddUserToDb(ap.userName, ap.passWord);
                if (succeeded)
                {
                    Console.WriteLine("Username: {0} Password: {1} has been registered successfully", ap.userName, ap.passWord);
                    SubPacket success = new SubPacket(GamePacketOpCode.Success, 0, 0, System.Text.Encoding.Unicode.GetBytes("Registration Successful"), SubPacketTypes.GamePacket);
                    BasePacket basePacket = BasePacket.CreatePacket(success, true, false);
                    client.QueuePacket(basePacket);
                }
                else
                {
                    ErrorPacket errorPacket = new ErrorPacket(ErrorCodes.DuplicateAccount, "Account already registered");
                    QueueErrorPacket(errorPacket, client);
                }


            }
        }

        private void QueueErrorPacket(ErrorPacket errorPacket, ClientConnection client)
        {
            SubPacket subPacket = errorPacket.buildPacket();
            BasePacket errorBasePacket = BasePacket.CreatePacket(subPacket, false, false);
            client.QueuePacket(errorBasePacket);
        }





    }
}
