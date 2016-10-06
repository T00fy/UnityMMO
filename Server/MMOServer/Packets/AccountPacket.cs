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
        public ushort lengthOfUserName;
        public ushort lengthOfPassword;
        public string userName;
        public string password;
        public int debug;

        public void Read(byte[] header, byte[] data)
        {

            //reading the header
            MemoryStream mem = new MemoryStream(header);
            BinaryReader binReader = new BinaryReader(mem);
                {
                    try
                    {

                    //GetAccountHeaderBytes() seems to use big endian, possible marshal class
                    //thats why have to reverse the array below  

                    register = binReader.ReadBoolean();
                    lengthOfUserName = SwapEndianShort(binReader.ReadBytes(sizeof(ushort)));
                    lengthOfPassword = SwapEndianShort(binReader.ReadBytes(sizeof(ushort)));

                        if (lengthOfUserName < 3 || lengthOfPassword < 3)
                        {
                            throw new Exception("invalid packet");
                        }
                    }
                    catch (Exception)
                    {
                        invalidPacket = true;
                        Console.WriteLine("Packet was invalid, check length of username/pw");
                    }
                }


            //reading the data
            if (!invalidPacket)
            {
                MemoryStream dataMem = new MemoryStream(data);
                BinaryReader binReaderData = new BinaryReader(dataMem);
                try
                {
                    //encoding unicode class uses little endian so can directly convert
                    userName = Encoding.Unicode.GetString(binReaderData.ReadBytes(lengthOfUserName*2)); //2 bytes per char
                    password = Encoding.Unicode.GetString(binReaderData.ReadBytes(lengthOfPassword*2));
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

        private ushort SwapEndianShort(byte[] bytes)
        {
            Array.Reverse(bytes);
            ushort converted = BitConverter.ToUInt16(bytes, 0);
            return converted;
        }

  /*      private string SwapEndianString(byte[] bytes)
        {
            Array.Reverse(bytes);
            string converted = Encoding.Unicode.GetString(bytes);
            return converted;
        }*/


        public byte[] GetDataBytes(string userName, string password)
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(mem);
            byte[] un = Encoding.Unicode.GetBytes(userName);
            debug = un.Length;
            byte[] pw = Encoding.Unicode.GetBytes(password);
            byte[] data = new byte[2*(userName.Length + password.Length) + 10]; //2 bytes per character i believe plus some clearance space

            try
            {
                //actual data
                bw.Write(un);
                bw.Write(pw);
                data = mem.GetBuffer();

                mem.Dispose();
                bw.Close();

            }
            catch (Exception)
            {
                Console.WriteLine("something went wrong in writing to account packet, check buffers");
            }
            return data;
        }
    }
}
