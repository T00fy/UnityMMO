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
        private string errorMessage;
        private uint errorId;


        public ErrorPacket(ErrorCodes errorId, string message)
        {
            errorMessage = message;
            this.errorId = (uint)errorId;
        }

        public SubPacket buildPacket()
        {
            MemoryStream memStream = new MemoryStream();
            BinaryWriter binWriter = new BinaryWriter(memStream);
            try
            {
                binWriter.Write(errorId);
                binWriter.Write(errorMessage);
                byte[] data = memStream.GetBuffer();
                memStream.Dispose();
                binWriter.Close();
                SubPacket subPacket = new SubPacket(GamePacketOpCode.Error, 0, 0, data, SubPacketTypes.GamePacket);
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
