using MMOServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MMOServer
{
    /// <summary>
    /// Used to query the server for the list of characters associated with the account
    /// </summary>
    public class CharacterQueryPacket
    {
        private uint charId;
        private uint accountId;
        private string name;
        private ushort strength;
        private ushort agility;
        private ushort intellect;
        private ushort vitality;
        private ushort dexterity;
        private ushort characterSlot;

        private string accountName;

        public CharacterQueryPacket() { }

        public CharacterQueryPacket(string accountName)
        {
            this.accountName = accountName;
        }

        public SubPacket BuildQueryPacket()
        {
            var bytes = Encoding.Unicode.GetBytes(accountName);
            SubPacket sp = new SubPacket(GamePacketOpCode.CharacterListQuery, 0, 0, bytes, SubPacketTypes.GamePacket);
            return sp;
        }

        public string ReadAccountName(SubPacket sp)
        {
            return Encoding.Unicode.GetString(sp.data);
        }

        public List<SubPacket> BuildResponsePacket(List<string[]> characterList)
        {

            var subPacketList = new List<SubPacket>();
            if (characterList != null)
            {
                int amountOfCharacters = characterList.Count;
                
                if (amountOfCharacters < 1)
                {
                    SubPacket s = new SubPacket(GamePacketOpCode.CharacterListQuery, 0, 0, BitConverter.GetBytes(-1), SubPacketTypes.GamePacket);
                    subPacketList.Add(s);
                    return subPacketList;
                }
                List<byte[]> list = new List<byte[]>();
                foreach (string[] s in characterList)
                {
                    charId = uint.Parse(s[0]);
                    characterSlot = ushort.Parse(s[1]);
                    accountId = uint.Parse(s[2]);
                    name = s[3];
                    strength = ushort.Parse(s[4]);
                    agility = ushort.Parse(s[5]);
                    intellect = ushort.Parse(s[6]);
                    vitality = ushort.Parse(s[7]);
                    dexterity = ushort.Parse(s[8]);

                    MemoryStream mem = new MemoryStream();
                    BinaryWriter bw = new BinaryWriter(mem);

                    byte[] charBytes = BitConverter.GetBytes(charId);
                    byte[] characterSlotBytes = BitConverter.GetBytes(characterSlot);
                    byte[] accountIdBytes = BitConverter.GetBytes(accountId);
                    byte[] nameBytes = Encoding.Unicode.GetBytes(name);
                    byte[] nameLengthBytes = BitConverter.GetBytes((ushort)nameBytes.Length);
                    byte[] strengthBytes = BitConverter.GetBytes(strength);
                    byte[] agiBytes = BitConverter.GetBytes(agility);
                    byte[] intBytes = BitConverter.GetBytes(intellect);
                    byte[] vitBytes = BitConverter.GetBytes(vitality);
                    byte[] dexBytes = BitConverter.GetBytes(dexterity);

                    byte[] data = new byte[charBytes.Length + characterSlotBytes.Length + accountIdBytes.Length + nameLengthBytes.Length + nameBytes.Length + strengthBytes.Length
                        + agiBytes.Length + intBytes.Length + vitBytes.Length + dexBytes.Length];

                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(charBytes);
                        Array.Reverse(characterSlotBytes);
                        Array.Reverse(accountIdBytes);
                        Array.Reverse(nameLengthBytes);
                        Array.Reverse(nameBytes);
                        Array.Reverse(strengthBytes);
                        Array.Reverse(agiBytes);
                        Array.Reverse(intBytes);
                        Array.Reverse(vitBytes);
                        Array.Reverse(dexBytes);

                    }

                    try
                    {
                        bw.Write(charBytes);
                        bw.Write(characterSlotBytes);
                        bw.Write(accountIdBytes);
                        bw.Write(nameLengthBytes);
                        bw.Write(nameBytes);
                        bw.Write(strengthBytes);
                        bw.Write(agiBytes);
                        bw.Write(intBytes);
                        bw.Write(vitBytes);
                        bw.Write(dexBytes);

                        data = mem.GetBuffer();

                        mem.Dispose();
                        bw.Close();
                    }

                    catch (Exception)
                    {
                        Console.WriteLine("Something went wrong with character creation packet");
                    }


                    list.Add(data);
                }

                foreach (var bytes in list)
                {
                    SubPacket sub = new SubPacket(GamePacketOpCode.CharacterListQuery, 0, 0, bytes, SubPacketTypes.GamePacket);
                    subPacketList.Add(sub);
                }
            }

            //at end add a packet with just -1, to indicate that it's the fin
            SubPacket temp = new SubPacket(GamePacketOpCode.CharacterListQuery, 0, 0, BitConverter.GetBytes(-1), SubPacketTypes.GamePacket);
            subPacketList.Add(temp);
            Console.WriteLine("Writing -1 to indicate end");
            
            return subPacketList;
        }

        public void ReadResponsePacket(SubPacket characterPacket)
        {
            MemoryStream mem = new MemoryStream(characterPacket.data);
            BinaryReader binReader = new BinaryReader(mem);
            try
            {
                charId = BitConverter.ToUInt32(binReader.ReadBytes(sizeof(uint)), 0);
                characterSlot = BitConverter.ToUInt16(binReader.ReadBytes(sizeof(ushort)), 0);
                accountId = BitConverter.ToUInt32(binReader.ReadBytes(sizeof(uint)), 0);
                var nameLength = BitConverter.ToUInt16(binReader.ReadBytes(sizeof(ushort)), 0);
                name = Encoding.Unicode.GetString(binReader.ReadBytes(nameLength));
                strength = BitConverter.ToUInt16(binReader.ReadBytes(sizeof(ushort)), 0); 
                agility = BitConverter.ToUInt16(binReader.ReadBytes(sizeof(ushort)), 0);
                intellect = BitConverter.ToUInt16(binReader.ReadBytes(sizeof(ushort)), 0);
                vitality = BitConverter.ToUInt16(binReader.ReadBytes(sizeof(ushort)), 0);
                dexterity = BitConverter.ToUInt16(binReader.ReadBytes(sizeof(ushort)), 0);

            }
            catch (Exception e)
            {
                Console.WriteLine("Error in reading handshake packet: " + e.Message);

            }

        }

        public ushort GetStrength() { return strength; }
        public ushort GetAgility() { return agility; }
        public ushort GetIntellect() { return intellect; }
        public ushort GetVitalty() { return vitality; }
        public ushort GetDexterity() { return dexterity; }
        public string GetName() { return name; }
        public uint GetCharId() { return charId; }
        public uint GetAccountId() { return accountId; }
        public ushort GetCharacterSlot() { return characterSlot; }

    }
}
