using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MMOServer
{
    public class AccountPacket
    {
        public bool invalidPacket = false;
        public bool register = false;
        public uint lengthOfUserName;
        public uint lengthOfPassword;
        public string userName;
        public string passWord;

        public void Read(byte[] header, byte[] data)
        {
            MemoryStream mem = new MemoryStream(header);
            BinaryReader binReader = new BinaryReader(mem);
                {
                    try
                    {
                        register = binReader.ReadBoolean();
                        lengthOfUserName = binReader.ReadUInt32();
                        lengthOfPassword = binReader.ReadUInt32();
                    }
                    catch (Exception)
                    {
                        invalidPacket = true;
                    }
                }

            if (!invalidPacket)
            {
                MemoryStream dataMem = new MemoryStream(data);
                BinaryReader binReaderData = new BinaryReader(dataMem);
                try
                {
                    userName = Encoding.ASCII.GetString(binReaderData.ReadBytes((int)lengthOfUserName));
                    passWord = Encoding.ASCII.GetString(binReaderData.ReadBytes((int)lengthOfPassword));
                }
                catch (Exception)
                {
                    invalidPacket = true;
                }
                dataMem.Dispose();
                binReaderData.Close();
            }
            mem.Dispose();
            binReader.Close();
        }

        public byte[] GetDataBytes(string userName, string passWord)
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(mem);
            byte[] data = new byte[2*(userName.Length + passWord.Length) + 10]; //2 bytes per character i believe plus some clearance space

            try
            {
                bw.Write(userName);
                bw.Write(passWord);
                data = mem.GetBuffer();

                mem.Dispose();
                bw.Close();

                return data;

            }
            catch (Exception)
            {
                Console.WriteLine("something went wrong in writing to account packet, check buffers");
            }
            throw new Exception("Error in GetDataBytes in Account Packet class, check buffers");

        }
    }
}
