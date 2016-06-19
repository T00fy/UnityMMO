using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//need to implement a logging system for packets library as console prints wont be written to unity
namespace MMOServer
{
    public class ErrorPacket
    {
        public string errorMessage;
        public uint errorId;

        public string GetErrorMessage()
        {
            return errorMessage;
        }

        public uint GetErrorId()
        {
            return errorId;
        }

        public void ReadPacket(byte[] data)
        {
            MemoryStream mem = new MemoryStream();
            BinaryReader bReader = new BinaryReader(mem);

            try
            {

                // need to fix this, doesn't return anything meaningful
                errorId = SwapEndianUInt(bReader.ReadBytes(sizeof(uint)));
                int count = SwapEndianInt(bReader.ReadBytes(sizeof(int)));
                Console.WriteLine(count);
                errorMessage = Encoding.Unicode.GetString(bReader.ReadBytes(count));

                mem.Dispose();
                bReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private int SwapEndianInt(byte[] bytes)
        {
            Array.Reverse(bytes);
            var converted = BitConverter.ToInt32(bytes, 0);
            return converted;
        }

        private uint SwapEndianUInt(byte[] bytes)
        {
            Array.Reverse(bytes);
            var converted = BitConverter.ToUInt32(bytes, 0);
            return converted;
        }

        public SubPacket buildPacket(ErrorCodes errorId, string message)
        {
            byte[] msg = Encoding.Unicode.GetBytes(message);
            MemoryStream memStream = new MemoryStream();
            BinaryWriter binWriter = new BinaryWriter(memStream);

            try
            {
                binWriter.Write((uint)errorId);
                binWriter.Write(msg.Length);
                binWriter.Write(msg);

                byte[] data = memStream.GetBuffer();
                memStream.Dispose();
                binWriter.Close();
                SubPacket subPacket = new SubPacket(GamePacketOpCode.AccountError, 0, 0, data, SubPacketTypes.GamePacket);
                return subPacket;
            }
            catch (Exception)
            {
                Console.WriteLine("something went wrong in writing to error packet, check buffers");
            }
            throw new Exception("something went wrong in writing to error packet, check buffers");
        }
    }
}
