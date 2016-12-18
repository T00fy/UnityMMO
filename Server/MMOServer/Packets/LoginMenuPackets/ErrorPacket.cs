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
            MemoryStream mem = new MemoryStream(data); //NEED TO PASS IN BYTE ARRAY WHEN READING!!! FML
            BinaryReader bReader = new BinaryReader(mem);

            try
            {
                errorId = bReader.ReadUInt16(); //for some reason don't have to swap endian no fucking idea why
                errorMessage = Encoding.Unicode.GetString(bReader.ReadBytes(data.Length));

                mem.Dispose();
                bReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public SubPacket buildPacket(GamePacketOpCode opCode, ErrorCodes errorId, string message)
        {
            byte[] msg = Encoding.Unicode.GetBytes(message);
            var errorIdConv = (ushort)errorId;
            MemoryStream memStream = new MemoryStream();
            BinaryWriter binWriter = new BinaryWriter(memStream);

            try
            {
                binWriter.Write(errorIdConv);
                binWriter.Write(msg);

                byte[] data = memStream.GetBuffer();
                memStream.Dispose();
                binWriter.Close();
                SubPacket subPacket = new SubPacket(opCode, 0, 0, data, SubPacketTypes.ErrorPacket);
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
