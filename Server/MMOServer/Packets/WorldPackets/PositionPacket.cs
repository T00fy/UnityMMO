using System;
using System.IO;

namespace MMOServer
{
    public class PositionPacket
    {

        public float XPos { get; set; }
        public float YPos { get; set; }
        public uint CharacterId { get; set; }



        /// <summary>
        /// Used by the client
        /// </summary>
        public PositionPacket(float xPos, float yPos, uint characterId)
        {
            XPos = xPos;
            YPos = yPos;
            CharacterId = characterId;
        }

        /// <summary>
        /// Used by the server
        /// </summary>
        public PositionPacket(byte[] data)
        {
            MemoryStream mem = new MemoryStream(data);
            BinaryReader br = new BinaryReader(mem);
            try
            {
                XPos = BitConverter.ToSingle(br.ReadBytes(sizeof(float)),0);
                YPos = BitConverter.ToSingle(br.ReadBytes(sizeof(float)),0);
                CharacterId = BitConverter.ToUInt32(br.ReadBytes(sizeof(uint)), 0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in reading Position packet: " + e.Message);

            }
            mem.Dispose();
            mem.Close();
        }

        public byte[] GetBytes()
        {
            byte[] xPosBytes = BitConverter.GetBytes(XPos);
            byte[] yPosBytes = BitConverter.GetBytes(YPos);
            byte[] characterIdBytes = BitConverter.GetBytes(CharacterId);

            return Utils.CombineBytes(xPosBytes, yPosBytes, characterIdBytes);
        }
    }
}