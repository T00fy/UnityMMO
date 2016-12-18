using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MMOServer
{
    public class CharacterCreatePacket    
    {
        public string characterName;
        private ushort nameLength;
        public ushort str;
        public ushort agi;
        public ushort inte;
        public ushort vit;
        public ushort dex;
        public ushort statsAllowed;
        public ushort selectedSlot;

        public CharacterCreatePacket(string characterName, ushort[] stats, ushort statsAllowed, ushort selectedSlot) {
            nameLength = (ushort)characterName.Length;
            this.characterName = characterName;
            str = stats[0];
            agi = stats[1];
            inte = stats[2];
            vit = stats[3];
            dex = stats[4];
            this.statsAllowed = statsAllowed;
            this.selectedSlot = selectedSlot;
        }

        public CharacterCreatePacket(byte[] receivedData)
        {
            Read(receivedData);
        }

        private void Read(byte[] receivedData)
        {
            MemoryStream mem = new MemoryStream(receivedData);
            BinaryReader binReader = new BinaryReader(mem);
            try
            {
                ushort lengthOfNextBytes = BitConverter.ToUInt16(binReader.ReadBytes(sizeof(ushort)),0);
                characterName = Encoding.Unicode.GetString(binReader.ReadBytes(lengthOfNextBytes));
                str = BitConverter.ToUInt16(binReader.ReadBytes(sizeof(ushort)), 0);
                agi = BitConverter.ToUInt16(binReader.ReadBytes(sizeof(ushort)), 0);
                inte = BitConverter.ToUInt16(binReader.ReadBytes(sizeof(ushort)), 0);
                vit = BitConverter.ToUInt16(binReader.ReadBytes(sizeof(ushort)), 0);
                dex = BitConverter.ToUInt16(binReader.ReadBytes(sizeof(ushort)), 0);
                statsAllowed = BitConverter.ToUInt16(binReader.ReadBytes(sizeof(ushort)), 0);
                selectedSlot = BitConverter.ToUInt16(binReader.ReadBytes(sizeof(ushort)), 0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in characterpacket");
                Console.WriteLine(e);
            }
        }

        public byte[] GetData()
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(mem);
            byte[] nameBytes = Encoding.Unicode.GetBytes(characterName);
            byte[] nameLengthBytes = BitConverter.GetBytes((ushort)nameBytes.Length);
            byte[] strBytes = BitConverter.GetBytes(str);
            byte[] agiBytes = BitConverter.GetBytes(agi);
            byte[] inteBytes = BitConverter.GetBytes(inte);
            byte[] vitBytes = BitConverter.GetBytes(vit);
            byte[] dexBytes = BitConverter.GetBytes(dex);
            byte[] statsAllowedBytes = BitConverter.GetBytes(statsAllowed);
            byte[] characterSlotBytes = BitConverter.GetBytes(selectedSlot);

            byte[] data = new byte[nameLengthBytes.Length + nameBytes.Length + strBytes.Length + agiBytes.Length + inteBytes.Length + vitBytes.Length + dexBytes.Length + statsAllowedBytes.Length + characterSlotBytes.Length];
            //should only need to do this for bitconverter class
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(nameLengthBytes);
                Array.Reverse(strBytes);
                Array.Reverse(agiBytes);
                Array.Reverse(inteBytes);
                Array.Reverse(vitBytes);
                Array.Reverse(dexBytes);
                Array.Reverse(statsAllowedBytes);
                Array.Reverse(characterSlotBytes);

            }

            try
            {
                bw.Write(nameLengthBytes);
                bw.Write(nameBytes);
                bw.Write(strBytes);
                bw.Write(agiBytes);
                bw.Write(inteBytes);
                bw.Write(vitBytes);
                bw.Write(dexBytes);
                bw.Write(statsAllowedBytes);
                bw.Write(characterSlotBytes);

                data = mem.GetBuffer();

                mem.Dispose();
                bw.Close();
            }

            catch (Exception)
            {
                Console.WriteLine("Something went wrong with character creation packet");
            }
            return data;
        }

        public string GetCharacterName()
        {
            return characterName;
        }
        public ushort GetStr()
        {
            return str;
        }
        public ushort GetAgi()
        {
            return agi;
        }
        public ushort GetInt()
        {
            return inte;
        }
        public ushort GetVit()
        {
            return vit;
        }
        public ushort GetDex()
        {
            return dex;
        }
        public ushort GetStatsAllowed()
        {
            return statsAllowed;
        }
        public ushort GetCharacterSlot()
        {
            return selectedSlot;
        }


    }
}
