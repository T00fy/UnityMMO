using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public AccountPacket(byte[] header, byte[] data)
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
                binReaderData.Dispose();
            }
            mem.Dispose();
            binReader.Dispose();
        }
    }
}
