using System;
using System.IO;

namespace MMOServer
{
    public class PositionPacket
    {

        public float XPos { get; set; }
        public float YPos { get; set; }
        public uint ActorId { get; set; }
        public bool Playable { get; set; }



        /// <summary>
        /// Used by the client
        /// </summary>
        public PositionPacket(float xPos, float yPos, bool playable, uint actorId)
        {
            XPos = xPos;
            YPos = yPos;
            Playable = playable;
            ActorId = actorId;
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
                Playable = BitConverter.ToBoolean(br.ReadBytes(sizeof(bool)), 0);
                ActorId = BitConverter.ToUInt32(br.ReadBytes(sizeof(uint)), 0);
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
            byte[] playable = BitConverter.GetBytes(Playable);
            byte[] characterIdBytes = BitConverter.GetBytes(ActorId);

            return Utils.CombineBytes(xPosBytes, yPosBytes, playable, characterIdBytes);
        }
    }
}