using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MMOServer
{

    public class CharacterDeletePacket
    {
        private uint charId;

        public CharacterDeletePacket(SubPacket receivedPacket)
        {
            try
            {
                charId = BitConverter.ToUInt32(receivedPacket.data, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in reading data from character delete packet");
                Console.WriteLine(e.ToString());
            }
            
        }

        public CharacterDeletePacket(uint charId)
        {
            this.charId = charId; 
        }

        public uint CharId
        {
            get
            {
                return charId;
            }

            set
            {
                charId = value;
            }
        }

        public SubPacket GetQueryPacket()
        {
            var bytes = BitConverter.GetBytes(charId);
            SubPacket sp = new SubPacket(GamePacketOpCode.CharacterDeleteQuery, 0, 0, bytes, SubPacketTypes.GamePacket);
            return sp;

        }
    }
}
