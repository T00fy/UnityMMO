using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MMOServer
{
    public class AcknowledgePacket
    {
        private int characterId;
        private string clientAddress;
        private bool ackSuccessful;

        public bool AckSuccessful
        {
            get
            {
                return ackSuccessful;
            }

            set
            {
                ackSuccessful = value;
            }
        }

        public int CharacterId
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

        public string ClientAddress
        {
            get
            {
                return clientAddress;
            }

            set
            {
                clientAddress = value;
            }
        }

        public AcknowledgePacket(bool ackSuccessful, string clientAddress, int characterId)
        {
            this.ackSuccessful = ackSuccessful;
            this.clientAddress = clientAddress;
            this.characterId = characterId;
        }

        public AcknowledgePacket(byte[] received)
        {
            MemoryStream mem = new MemoryStream(received);
            BinaryReader br = new BinaryReader(mem);
            try
            {
                ackSuccessful = BitConverter.ToBoolean(br.ReadBytes(sizeof(bool)), 0);
                var lengthAddress = BitConverter.ToUInt16(br.ReadBytes(sizeof(ushort)), 0);
                clientAddress = Encoding.Unicode.GetString(br.ReadBytes(lengthAddress));
                characterId = BitConverter.ToInt32(br.ReadBytes(sizeof(int)), 0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in reading ack packet: " + e.Message);

            }
        }

        public byte[] GetBytes()
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(mem);
            byte[] successBytes = BitConverter.GetBytes(ackSuccessful);
            byte[] addressBytes = Encoding.Unicode.GetBytes(clientAddress);
            byte[] addressLengthBytes = BitConverter.GetBytes((ushort)addressBytes.Length);
            byte[] characterIdBytes = BitConverter.GetBytes(characterId);

            byte[] data = new byte[successBytes.Length + addressLengthBytes.Length + addressBytes.Length + characterIdBytes.Length];

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(successBytes);
                Array.Reverse(addressLengthBytes);
                Array.Reverse(characterIdBytes);
            }

            try
            {
                bw.Write(successBytes);
                bw.Write(addressLengthBytes);
                bw.Write(addressBytes);
                bw.Write(characterIdBytes);
                data = mem.GetBuffer();
                mem.Dispose();
                mem.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong with AckPacket");
                Console.WriteLine(e.ToString());
            }
            return data;
        }
    }
}
