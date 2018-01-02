using System;
using System.IO;

namespace MMOServer
{
    public class DisconnectPacket
    {

        public DisconnectPacket(byte[] data)
        {
            MemoryStream mem = new MemoryStream(data);
            BinaryReader br = new BinaryReader(mem);
            try
            {
                SessionId = BitConverter.ToUInt32(br.ReadBytes(sizeof(uint)), 0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in reading disconnection packet: " + e.Message);

            }
            mem.Dispose();
            mem.Close();
        }

        public DisconnectPacket(uint sessionId)
        {
            this.SessionId = sessionId;
        }

        public uint SessionId { get; set; }

        public byte[] GetBytes()
        {
            return BitConverter.GetBytes(SessionId);
        }


    }
}