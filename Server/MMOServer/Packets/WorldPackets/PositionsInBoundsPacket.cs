using System;
using System.IO;

namespace MMOServer
{
    public class PositionsInBoundsPacket
    {
        public float XMin { get; set; }
        public float XMax { get; set; }
        public float YMin { get; set; }
        public float YMax { get; set; }

        public PositionsInBoundsPacket(float xMin, float xMax, float yMin, float yMax )
        {
            XMin = xMin;
            XMax = xMax;
            YMin = yMin;
            YMax = yMax;
        }

        public PositionsInBoundsPacket(byte[] data)
        {
            MemoryStream mem = new MemoryStream(data);
            BinaryReader br = new BinaryReader(mem);
            try
            {
                XMin = BitConverter.ToSingle(br.ReadBytes(sizeof(float)), 0);
                XMax = BitConverter.ToSingle(br.ReadBytes(sizeof(float)), 0);
                YMin = BitConverter.ToSingle(br.ReadBytes(sizeof(float)), 0);
                YMax = BitConverter.ToSingle(br.ReadBytes(sizeof(float)), 0);
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
            byte[] xMinBytes = BitConverter.GetBytes(XMin);
            byte[] xMaxBytes = BitConverter.GetBytes(XMax);
            byte[] yMinBytes = BitConverter.GetBytes(YMin);
            byte[] yMaxBytes = BitConverter.GetBytes(YMax);

            return Utils.CombineBytes(xMinBytes, xMaxBytes, yMinBytes, yMaxBytes);
        }

    }
}