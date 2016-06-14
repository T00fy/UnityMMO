using System;
using System.Runtime.InteropServices;

namespace MMOServer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SubPacketHeader
    {
        public ushort subpacketSize;
        public ushort type;
        public uint sourceId;
        public uint targetId;
        public uint subpacketMisc;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GameMessageHeader
    {
        public ushort opcode;
        public uint timestamp;
        public uint gamepacketMisc;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AccountHeader
    {
        public byte setForRegister;
        public uint lengthOfUserName;
        public uint lengthOfPassword;
    }

    public class SubPacket
    {
        public const int SUBPACKET_SIZE = 0x10; //16 in decimal
        public const int GAMEMESSAGE_SIZE = 0x10;
        public const int ACCOUNTMESSAGE_SIZE = 0x10;

        public SubPacketHeader header;
        public GameMessageHeader gameMessage;
        public AccountHeader accountHeader;
        public byte[] data;

        public unsafe SubPacket(byte[] bytes, ref int offset)
        {
            if (bytes.Length < offset + SUBPACKET_SIZE)
                throw new OverflowException("Packet Error: Subpacket was too small");

            fixed (byte* pdata = &bytes[offset])
            {
                header = (SubPacketHeader)Marshal.PtrToStructure(new IntPtr(pdata), typeof(SubPacketHeader));
            }

            if (header.type == (ushort)SubPacketTypes.GamePacket)
            {
                fixed (byte* pdata = &bytes[offset + GAMEMESSAGE_SIZE])
                {
                    gameMessage = (GameMessageHeader)Marshal.PtrToStructure(new IntPtr(pdata), typeof(GameMessageHeader));
                }
            }

            if (header.type == (ushort)SubPacketTypes.Account)
            {
                fixed (byte* pdata = &bytes[offset + ACCOUNTMESSAGE_SIZE])
                {
                    accountHeader = (AccountHeader)Marshal.PtrToStructure(new IntPtr(pdata), typeof(AccountHeader));
                }
            }

            if (bytes.Length < offset + header.subpacketSize)
                throw new OverflowException("Packet Error: Subpacket size didn't equal subpacket data");

            if (header.type == (ushort)SubPacketTypes.GamePacket) // if type in the header is a game packet copy the gamepacket header into 16bit array
            {
                data = new byte[header.subpacketSize - SUBPACKET_SIZE - GAMEMESSAGE_SIZE];
                Array.Copy(bytes, offset + SUBPACKET_SIZE + GAMEMESSAGE_SIZE, data, 0, data.Length);
            }
            else if (header.type == (ushort)SubPacketTypes.Account)
            {
                data = new byte[header.subpacketSize - SUBPACKET_SIZE - ACCOUNTMESSAGE_SIZE];
                Array.Copy(bytes, offset + SUBPACKET_SIZE + ACCOUNTMESSAGE_SIZE, data, 0, data.Length);
            }
            else //else this is the payload
            {
                data = new byte[header.subpacketSize - SUBPACKET_SIZE];
                Array.Copy(bytes, offset + SUBPACKET_SIZE, data, 0, data.Length);
            }

            offset += header.subpacketSize;
        }

        //Shorthand for SubPacket with gamepacket type
        public SubPacket(GamePacketOpCode opCode, uint sourceId, uint targetId, byte[] data, SubPacketTypes spt)
        {
            header = new SubPacketHeader();
            gameMessage = new GameMessageHeader();

            gameMessage.opcode = (ushort)opCode;
            header.sourceId = sourceId;
            header.targetId = targetId;

            gameMessage.timestamp = Utils.UnixTimeStampUTC();

            header.type = (ushort)spt;
            header.subpacketMisc = 0x00;
            gameMessage.gamepacketMisc = 0x0;

            this.data = data;

            header.subpacketSize = (ushort)(SUBPACKET_SIZE + GAMEMESSAGE_SIZE + data.Length);
        }

        //Shorthand for SubPacket with account type
   /*     public SubPacket(bool register, uint lengthOfUsername, uint lengthOfPassword, uint sourceId, uint targetId, byte[] data, SubPacketTypes spt)
        {
            header = new SubPacketHeader();
            accountHeader = new AccountHeader();
            if (register)
            {
                accountHeader.setForRegister = 1;
            }
            else
            {
                accountHeader.setForRegister = 0;
            }
            accountHeader.lengthOfUserName = lengthOfUsername;
            accountHeader.lengthOfPassword = lengthOfPassword;
            header.sourceId = sourceId;
            header.targetId = targetId;

            header.type = (ushort)spt;
            header.subpacketMisc = 0x00;

            this.data = data;

            header.subpacketSize = (ushort)(SUBPACKET_SIZE + GAMEMESSAGE_SIZE + data.Length);
        }*/

        public SubPacket(SubPacket original, uint newTargetId)
        {
            header = new SubPacketHeader();
            gameMessage = original.gameMessage;
            header.subpacketSize = original.header.subpacketSize;
            header.type = original.header.type;
            header.sourceId = original.header.sourceId;
            header.targetId = newTargetId;
            data = original.data;
        }

        public byte[] getHeaderBytes()
        {
            int size = Marshal.SizeOf(header);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(header, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public byte[] getGameMessageBytes()
        {
            int size = Marshal.SizeOf(gameMessage);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(gameMessage, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public byte[] getBytes()
        {
            byte[] outBytes = new byte[header.subpacketSize];
            Array.Copy(getHeaderBytes(), 0, outBytes, 0, SUBPACKET_SIZE);

            if (header.type == (ushort)SubPacketTypes.GamePacket)
                Array.Copy(getGameMessageBytes(), 0, outBytes, SUBPACKET_SIZE, GAMEMESSAGE_SIZE);

            //if the header type field in the pcaket is the gamepacket, add GAMEMESSAGE_SIZE, otherwise add nothing)
            Array.Copy(data, 0, outBytes, SUBPACKET_SIZE + (header.type == (ushort)SubPacketTypes.GamePacket ? GAMEMESSAGE_SIZE : 0), data.Length);
            return outBytes;
        }
        
        public void debugPrintSubPacket()
        {
#if DEBUG
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Size: 0x{0:X}", header.subpacketSize);
            if (header.type == (ushort)SubPacketTypes.GamePacket)
                Console.WriteLine("Opcode: 0x{0:X}", gameMessage.opcode);
            Console.WriteLine("{0}", Utils.ByteArrayToHex(getHeaderBytes()));
            if (header.type == (ushort)SubPacketTypes.GamePacket)
                Console.WriteLine("{0}", Utils.ByteArrayToHex(getGameMessageBytes()));
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("{0}", Utils.ByteArrayToHex(data));
            Console.BackgroundColor = ConsoleColor.Black;
#endif
        }
    }
}
