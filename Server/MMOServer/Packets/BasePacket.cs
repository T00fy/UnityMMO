using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace MMOServer
{

    [StructLayout(LayoutKind.Sequential)]
    public struct BasePacketHeader
    {
        public byte isAuthenticated;
        public byte isEncrypted;
        public ushort connectionType;
        public ushort packetSize; //Packet Size: The total size of the packet including header.
        public ushort numSubpackets;
        public ulong timestamp; //Miliseconds
    }

    public class BasePacket
    {
        public const int BASEPACKET_SIZE = 0x10;
        public const int TYPE_ZONE = 1;
        public const int TYPE_CHAT = 2;
        public BasePacketHeader header;
        public byte[] data;


        public bool isAuthenticated()
        {
            if (header.isAuthenticated == 0)
            {
                return false;
            }
            if (header.isAuthenticated == 1)
            {
                return true;
            }
            Console.WriteLine("something went wrong in basepacket authentication method");
            return false;
        }

        public unsafe BasePacket(byte[] bytes, ref int offset)
        {
            if (bytes.Length < offset + BASEPACKET_SIZE)
                throw new OverflowException("Packet Error: Packet was too small");

            fixed (byte* pdata = &bytes[offset])
            {
                header = (BasePacketHeader)Marshal.PtrToStructure(new IntPtr(pdata), typeof(BasePacketHeader));
            }

            int packetSize = header.packetSize;
            Console.WriteLine("packetSize: " + packetSize);
            if (bytes.Length < offset + header.packetSize)
                throw new OverflowException("Packet Error: Packet size didn't equal given size");

            data = new byte[packetSize - BASEPACKET_SIZE];
            Array.Copy(bytes, offset + BASEPACKET_SIZE, data, 0, packetSize - BASEPACKET_SIZE);

            offset += packetSize;
        }

        public BasePacket(BasePacketHeader header, byte[] data)
        {
            this.header = header;
            this.data = data;
        }

        public List<SubPacket> GetSubpackets()
        {
            List<SubPacket> subpackets = new List<SubPacket>(header.numSubpackets);

            int offset = 0;

            while (offset < data.Length)
                subpackets.Add(new SubPacket(data, ref offset));

            return subpackets;
        }

        public unsafe static BasePacketHeader GetHeader(byte[] bytes)
        {
            BasePacketHeader header;
            if (bytes.Length < BASEPACKET_SIZE)
                throw new OverflowException("Packet Error: Packet was too small");

            fixed (byte* pdata = &bytes[0])
            {
                header = (BasePacketHeader)Marshal.PtrToStructure(new IntPtr(pdata), typeof(BasePacketHeader));
            }

            return header;
        }

        public byte[] GetHeaderBytes()
        {
            int size = Marshal.SizeOf(header);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(header, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public byte[] GetPacketBytes()
        {
            byte[] outBytes = new byte[header.packetSize];
            Array.Copy(GetHeaderBytes(), 0, outBytes, 0, BASEPACKET_SIZE);
            Array.Copy(data, 0, outBytes, BASEPACKET_SIZE, data.Length);
            return outBytes;
        }

        #region Utility Functions
        public static BasePacket CreatePacket(List<SubPacket> subpackets, bool isAuthed, bool isEncrypted)
        {
            //Create Header
            BasePacketHeader header = new BasePacketHeader();
            byte[] data = null;

            header.isAuthenticated = isAuthed ? (byte)1 : (byte)0;
            header.isEncrypted = isEncrypted ? (byte)1 : (byte)0;
            header.numSubpackets = (ushort)subpackets.Count;
            header.packetSize = BASEPACKET_SIZE;
            header.timestamp = Utils.MilisUnixTimeStampUTC();

            //Get packet size
            foreach (SubPacket subpacket in subpackets)
                header.packetSize += subpacket.header.subpacketSize;

            data = new byte[header.packetSize - 0x10];

            //Add Subpackets
            int offset = 0;
            foreach (SubPacket subpacket in subpackets)
            {
                byte[] subpacketData = subpacket.getBytes();
                Array.Copy(subpacketData, 0, data, offset, subpacketData.Length);
                offset += (ushort)subpacketData.Length;
            }

            Debug.Assert(data != null && offset == data.Length && header.packetSize == 0x10 + offset);

            BasePacket packet = new BasePacket(header, data);
            return packet;
        }

        public static BasePacket CreatePacket(SubPacket subpacket, bool isAuthed, bool isEncrypted)
        {
            //Create Header
            BasePacketHeader header = new BasePacketHeader();
            byte[] data = null;

            header.isAuthenticated = isAuthed ? (byte)1 : (byte)0;
            header.isEncrypted = isEncrypted ? (byte)1 : (byte)0;
            header.numSubpackets = 1;
            header.packetSize = BASEPACKET_SIZE;
            header.timestamp = Utils.MilisUnixTimeStampUTC();

            //Get packet size
            header.packetSize += subpacket.header.subpacketSize;

            data = new byte[header.packetSize - 0x10];

            //Add Subpackets
            byte[] subpacketData = subpacket.getBytes();
            Array.Copy(subpacketData, 0, data, 0, subpacketData.Length);

            Debug.Assert(data != null);

            BasePacket packet = new BasePacket(header, data);
            return packet;
        }

        public static BasePacket CreatePacket(byte[] data, bool isAuthed, bool isEncrypted)
        {

            Debug.Assert(data != null);

            //Create Header
            BasePacketHeader header = new BasePacketHeader();

            header.isAuthenticated = isAuthed ? (byte)1 : (byte)0;
            header.isEncrypted = isEncrypted ? (byte)1 : (byte)0;
            header.numSubpackets = 1;
            header.packetSize = BASEPACKET_SIZE;
            header.timestamp = Utils.MilisUnixTimeStampUTC();

            //Get packet size
            header.packetSize += (ushort)data.Length;

            BasePacket packet = new BasePacket(header, data);
            return packet;
        }

        public static unsafe void EncryptPacket(Blowfish blowfish, BasePacket packet)
        {
            byte[] data = packet.data;
            int size = packet.header.packetSize;

            int offset = 0;
            while (offset < data.Length)
            {
                if (data.Length < offset + SubPacket.SUBPACKET_SIZE)
                    throw new OverflowException("Packet Error: Subpacket was too small");

                SubPacketHeader header;
                fixed (byte* pdata = &data[offset])
                {
                    header = (SubPacketHeader)Marshal.PtrToStructure(new IntPtr(pdata), typeof(SubPacketHeader));
                }

                if (data.Length < offset + header.subpacketSize)
                    throw new OverflowException("Packet Error: Subpacket size didn't equal subpacket data");

                blowfish.Encipher(data, offset + 0x10, header.subpacketSize - 0x10);

                offset += header.subpacketSize;
            }

        }

        public static unsafe void DecryptPacket(Blowfish blowfish, ref BasePacket packet)
        {
            byte[] data = packet.data;
            int size = packet.header.packetSize;

            int offset = 0;
            while (offset < data.Length)
            {
                if (data.Length < offset + SubPacket.SUBPACKET_SIZE)
                    throw new OverflowException("Packet Error: Subpacket was too small");

                SubPacketHeader header;
                fixed (byte* pdata = &data[offset])
                {
                    header = (SubPacketHeader)Marshal.PtrToStructure(new IntPtr(pdata), typeof(SubPacketHeader));
                }

                if (data.Length < offset + header.subpacketSize)
                    throw new OverflowException("Packet Error: Subpacket size didn't equal subpacket data");

                blowfish.Decipher(data, offset + 0x10, header.subpacketSize - 0x10);

                offset += header.subpacketSize;
            }
        }
        #endregion

        public void debugPrintPacket()
        {
#if DEBUG
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("IsAuthed: {0}, IsEncrypted: {1}, Size: 0x{2:X}, Num Subpackets: {3}", header.isAuthenticated, header.isEncrypted, header.packetSize, header.numSubpackets);
            foreach (SubPacket sub in GetSubpackets())
                sub.debugPrintSubPacket();
            Console.BackgroundColor = ConsoleColor.Black;
#endif
        }

    }
}