using System;
using System.IO;

namespace MMOServer
{
    public class PositionPacket
    {

        public float XPos { get; set; }
        public float YPos { get; set; }



        /// <summary>
        /// Used by the client
        /// </summary>
        public PositionPacket(float xPos, float yPos, float characterId)//add characterId?
        {
            XPos = xPos;
            YPos = yPos;
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

            return Utils.CombineBytes(xPosBytes, yPosBytes);
        }
    }
}