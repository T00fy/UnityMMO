using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MMOServer
{
    public class HandshakePacket
    {
        private string clientAddress;
        private int characterId;
        private int clientPort;

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

        public HandshakePacket(string clientAddress, int clientPort, int characterId)
        {
            this.clientAddress = clientAddress;
            this.characterId = characterId;
            this.clientPort = clientPort;
        }

        public HandshakePacket(byte[] received)
        {
            MemoryStream mem = new MemoryStream(received);
            BinaryReader br = new BinaryReader(mem);
            try
            {
                var lengthAddress = BitConverter.ToUInt16(br.ReadBytes(sizeof(ushort)), 0);
                clientAddress = Encoding.Unicode.GetString(br.ReadBytes(lengthAddress));
                characterId = BitConverter.ToInt32(br.ReadBytes(sizeof(int)), 0);
                clientPort = BitConverter.ToInt32(br.ReadBytes(sizeof(int)), 0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in reading character loading packets: " + e.Message);

            }

        }

        public byte[] GetBytes()
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(mem);
            byte[] addressBytes = Encoding.Unicode.GetBytes(clientAddress);
            byte[] addressLengthBytes = BitConverter.GetBytes((ushort)addressBytes.Length);
            byte[] characterIdBytes = BitConverter.GetBytes(characterId);
            byte[] portBytes = BitConverter.GetBytes(clientPort);

            byte[] data = new byte[addressLengthBytes.Length + addressBytes.Length + characterIdBytes.Length];

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(addressLengthBytes);
                Array.Reverse(characterIdBytes);
                Array.Reverse(portBytes);
            }

            try
            {
                bw.Write(addressLengthBytes);
                bw.Write(addressBytes);
                bw.Write(characterIdBytes);
                bw.Write(portBytes);
                data = mem.GetBuffer();
                mem.Dispose();
                mem.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong with handshakepacket");
                Console.WriteLine(e.ToString());
            }
            return data;
        }
    }
}
