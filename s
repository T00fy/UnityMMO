[1mdiff --git a/Server/MMOServer/MMOServer/AccountPacket.cs b/Server/MMOServer/MMOServer/AccountPacket.cs[m
[1mdeleted file mode 100644[m
[1mindex bd2729f..0000000[m
[1m--- a/Server/MMOServer/MMOServer/AccountPacket.cs[m
[1m+++ /dev/null[m
[36m@@ -1,56 +0,0 @@[m
[31m-﻿using System;[m
[31m-using System.Collections.Generic;[m
[31m-using System.Linq;[m
[31m-using System.Text;[m
[31m-using System.Threading.Tasks;[m
[31m-using System.IO;[m
[31m-[m
[31m-namespace MMOServer[m
[31m-{[m
[31m-    class AccountPacket[m
[31m-    {[m
[31m-        public bool invalidPacket = false;[m
[31m-        public bool register = false;[m
[31m-        public uint lengthOfUserName;[m
[31m-        public uint lengthOfPassword;[m
[31m-        public string userName;[m
[31m-        public string passWord;[m
[31m-[m
[31m-        public AccountPacket(byte[] header, byte[] data)[m
[31m-        {[m
[31m-            MemoryStream mem = new MemoryStream(header);[m
[31m-            BinaryReader binReader = new BinaryReader(mem);[m
[31m-                {[m
[31m-                    try[m
[31m-                    {[m
[31m-                        register = binReader.ReadBoolean();[m
[31m-                        lengthOfUserName = binReader.ReadUInt32();[m
[31m-                        lengthOfPassword = binReader.ReadUInt32();[m
[31m-                    }[m
[31m-                    catch (Exception)[m
[31m-                    {[m
[31m-                        invalidPacket = true;[m
[31m-                    }[m
[31m-                }[m
[31m-[m
[31m-            if (!invalidPacket)[m
[31m-            {[m
[31m-                MemoryStream dataMem = new MemoryStream(data);[m
[31m-                BinaryReader binReaderData = new BinaryReader(dataMem);[m
[31m-                try[m
[31m-                {[m
[31m-                    userName = Encoding.ASCII.GetString(binReaderData.ReadBytes((int)lengthOfUserName));[m
[31m-                    passWord = Encoding.ASCII.GetString(binReaderData.ReadBytes((int)lengthOfPassword));[m
[31m-                }[m
[31m-                catch (Exception)[m
[31m-                {[m
[31m-                    invalidPacket = true;[m
[31m-                }[m
[31m-                dataMem.Dispose();[m
[31m-                binReaderData.Dispose();[m
[31m-            }[m
[31m-            mem.Dispose();[m
[31m-            binReader.Dispose();[m
[31m-        }[m
[31m-    }[m
[31m-}[m
[1mdiff --git a/Server/MMOServer/MMOServer/BasePacket.cs b/Server/MMOServer/MMOServer/BasePacket.cs[m
[1mdeleted file mode 100644[m
[1mindex 0aeb0cc..0000000[m
[1m--- a/Server/MMOServer/MMOServer/BasePacket.cs[m
[1m+++ /dev/null[m
[36m@@ -1,267 +0,0 @@[m
[31m-﻿using System;[m
[31m-using System.Collections.Generic;[m
[31m-using System.Linq;[m
[31m-using System.Text;[m
[31m-using System.Threading.Tasks;[m
[31m-using System.Runtime.InteropServices;[m
[31m-using System.Diagnostics;[m
[31m-using System.IO;[m
[31m-[m
[31m-namespace MMOServer[m
[31m-{[m
[31m-[m
[31m-    [StructLayout(LayoutKind.Sequential)][m
[31m-    public struct BasePacketHeader[m
[31m-    {[m
[31m-        public byte isAuthenticated;[m
[31m-        public byte isEncrypted;[m
[31m-        public ushort connectionType;[m
[31m-        public ushort packetSize; //Packet Size: The total size of the packet including header.[m
[31m-        public ushort numSubpackets;[m
[31m-        public ulong timestamp; //Miliseconds[m
[31m-    }[m
[31m-[m
[31m-    public class BasePacket[m
[31m-    {[m
[31m-        public const int BASEPACKET_SIZE = 0x10;[m
[31m-[m
[31m-        public BasePacketHeader header;[m
[31m-        public byte[] data;[m
[31m-[m
[31m-[m
[31m-        public bool isAuthenticated()[m
[31m-        {[m
[31m-            if (header.isAuthenticated == 0)[m
[31m-            {[m
[31m-                return false;[m
[31m-            }[m
[31m-            if (header.isAuthenticated == 1)[m
[31m-            {[m
[31m-                return true;[m
[31m-            }[m
[31m-            Console.WriteLine("something went wrong in basepacket authentication method");[m
[31m-            return false;[m
[31m-        }[m
[31m-[m
[31m-        public unsafe BasePacket(byte[] bytes, ref int offset)[m
[31m-        {[m
[31m-            if (bytes.Length < offset + BASEPACKET_SIZE)[m
[31m-                throw new OverflowException("Packet Error: Packet was too small");[m
[31m-[m
[31m-            fixed (byte* pdata = &bytes[offset])[m
[31m-            {[m
[31m-                header = (BasePacketHeader)Marshal.PtrToStructure(new IntPtr(pdata), typeof(BasePacketHeader));[m
[31m-            }[m
[31m-[m
[31m-            int packetSize = header.packetSize;[m
[31m-[m
[31m-            if (bytes.Length < offset + header.packetSize)[m
[31m-                throw new OverflowException("Packet Error: Packet size didn't equal given size");[m
[31m-[m
[31m-            data = new byte[packetSize - BASEPACKET_SIZE];[m
[31m-            Array.Copy(bytes, offset + BASEPACKET_SIZE, data, 0, packetSize - BASEPACKET_SIZE);[m
[31m-[m
[31m-            offset += packetSize;[m
[31m-        }[m
[31m-[m
[31m-        public BasePacket(BasePacketHeader header, byte[] data)[m
[31m-        {[m
[31m-            this.header = header;[m
[31m-            this.data = data;[m
[31m-        }[m
[31m-[m
[31m-        public List<SubPacket> GetSubpackets()[m
[31m-        {[m
[31m-            List<SubPacket> subpackets = new List<SubPacket>(header.numSubpackets);[m
[31m-[m
[31m-            int offset = 0;[m
[31m-[m
[31m-            while (offset < data.Length)[m
[31m-                subpackets.Add(new SubPacket(data, ref offset));[m
[31m-[m
[31m-            return subpackets;[m
[31m-        }[m
[31m-[m
[31m-        public unsafe static BasePacketHeader GetHeader(byte[] bytes)[m
[31m-        {[m
[31m-            BasePacketHeader header;[m
[31m-            if (bytes.Length < BASEPACKET_SIZE)[m
[31m-                throw new OverflowException("Packet Error: Packet was too small");[m
[31m-[m
[31m-            fixed (byte* pdata = &bytes[0])[m
[31m-            {[m
[31m-                header = (BasePacketHeader)Marshal.PtrToStructure(new IntPtr(pdata), typeof(BasePacketHeader));[m
[31m-            }[m
[31m-[m
[31m-            return header;[m
[31m-        }[m
[31m-[m
[31m-        public byte[] GetHeaderBytes()[m
[31m-        {[m
[31m-            int size = Marshal.SizeOf(header);[m
[31m-            byte[] arr = new byte[size];[m
[31m-[m
[31m-            IntPtr ptr = Marshal.AllocHGlobal(size);[m
[31m-            Marshal.StructureToPtr(header, ptr, true);[m
[31m-            Marshal.Copy(ptr, arr, 0, size);[m
[31m-            Marshal.FreeHGlobal(ptr);[m
[31m-            return arr;[m
[31m-        }[m
[31m-[m
[31m-        public byte[] GetPacketBytes()[m
[31m-        {[m
[31m-            byte[] outBytes = new byte[header.packetSize];[m
[31m-            Array.Copy(GetHeaderBytes(), 0, outBytes, 0, BASEPACKET_SIZE);[m
[31m-            Array.Copy(data, 0, outBytes, BASEPACKET_SIZE, data.Length);[m
[31m-            return outBytes;[m
[31m-        }[m
[31m-[m
[31m-        #region Utility Functions[m
[31m-        public static BasePacket CreatePacket(List<SubPacket> subpackets, bool isAuthed, bool isEncrypted)[m
[31m-        {[m
[31m-            //Create Header[m
[31m-            BasePacketHeader header = new BasePacketHeader();[m
[31m-            byte[] data = null;[m
[31m-[m
[31m-            header.isAuthenticated = isAuthed ? (byte)1 : (byte)0;[m
[31m-            header.isEncrypted = isEncrypted ? (byte)1 : (byte)0;[m
[31m-            header.numSubpackets = (ushort)subpackets.Count;[m
[31m-            header.packetSize = BASEPACKET_SIZE;[m
[31m-            header.timestamp = Utils.MilisUnixTimeStampUTC();[m
[31m-[m
[31m-            //Get packet size[m
[31m-            foreach (SubPacket subpacket in subpackets)[m
[31m-                header.packetSize += subpacket.header.subpacketSize;[m
[31m-[m
[31m-            data = new byte[header.packetSize - 0x10];[m
[31m-[m
[31m-            //Add Subpackets[m
[31m-            int offset = 0;[m
[31m-            foreach (SubPacket subpacket in subpackets)[m
[31m-            {[m
[31m-                byte[] subpacketData = subpacket.getBytes();[m
[31m-                Array.Copy(subpacketData, 0, data, offset, subpacketData.Length);[m
[31m-                offset += (ushort)subpacketData.Length;[m
[31m-            }[m
[31m-[m
[31m-            Debug.Assert(data != null && offset == data.Length && header.packetSize == 0x10 + offset);[m
[31m-[m
[31m-            BasePacket packet = new BasePacket(header, data);[m
[31m-            return packet;[m
[31m-        }[m
[31m-[m
[31m-        public static BasePacket CreatePacket(SubPacket subpacket, bool isAuthed, bool isEncrypted)[m
[31m-        {[m
[31m-            //Create Header[m
[31m-            BasePacketHeader header = new BasePacketHeader();[m
[31m-            byte[] data = null;[m
[31m-[m
[31m-            header.isAuthenticated = isAuthed ? (byte)1 : (byte)0;[m
[31m-            header.isEncrypted = isEncrypted ? (byte)1 : (byte)0;[m
[31m-            header.numSubpackets = 1;[m
[31m-            header.packetSize = BASEPACKET_SIZE;[m
[31m-            header.timestamp = Utils.MilisUnixTimeStampUTC();[m
[31m-[m
[31m-            //Get packet size[m
[31m-            header.packetSize += subpacket.header.subpacketSize;[m
[31m-[m
[31m-            data = new byte[header.packetSize - 0x10];[m
[31m-[m
[31m-            //Add Subpackets[m
[31m-            byte[] subpacketData = subpacket.getBytes();[m
[31m-            Array.Copy(subpacketData, 0, data, 0, subpacketData.Length);[m
[31m-[m
[31m-            Debug.Assert(data != null);[m
[31m-[m
[31m-            BasePacket packet = new BasePacket(header, data);[m
[31m-            return packet;[m
[31m-        }[m
[31m-[m
[31m-        public static BasePacket CreatePacket(byte[] data, bool isAuthed, bool isEncrypted)[m
[31m-        {[m
[31m-[m
[31m-            Debug.Assert(data != null);[m
[31m-[m
[31m-            //Create Header[m
[31m-            BasePacketHeader header = new BasePacketHeader();[m
[31m-[m
[31m-            header.isAuthenticated = isAuthed ? (byte)1 : (byte)0;[m
[31m-            header.isEncrypted = isEncrypted ? (byte)1 : (byte)0;[m
[31m-            header.numSubpackets = 1;[m
[31m-            header.packetSize = BASEPACKET_SIZE;[m
[31m-            header.timestamp = Utils.MilisUnixTimeStampUTC();[m
[31m-[m
[31m-            //Get packet size[m
[31m-            header.packetSize += (ushort)data.Length;[m
[31m-[m
[31m-            BasePacket packet = new BasePacket(header, data);[m
[31m-            return packet;[m
[31m-        }[m
[31m-[m
[31m-        public static unsafe void EncryptPacket(Blowfish blowfish, BasePacket packet)[m
[31m-        {[m
[31m-            byte[] data = packet.data;[m
[31m-            int size = packet.header.packetSize;[m
[31m-[m
[31m-            int offset = 0;[m
[31m-            while (offset < data.Length)[m
[31m-            {[m
[31m-                if (data.Length < offset + SubPacket.SUBPACKET_SIZE)[m
[31m-                    throw new OverflowException("Packet Error: Subpacket was too small");[m
[31m-[m
[31m-                SubPacketHeader header;[m
[31m-                fixed (byte* pdata = &data[offset])[m
[31m-                {[m
[31m-                    header = (SubPacketHeader)Marshal.PtrToStructure(new IntPtr(pdata), typeof(SubPacketHeader));[m
[31m-                }[m
[31m-[m
[31m-                if (data.Length < offset + header.subpacketSize)[m
[31m-                    throw new OverflowException("Packet Error: Subpacket size didn't equal subpacket data");[m
[31m-[m
[31m-                blowfish.Encipher(data, offset + 0x10, header.subpacketSize - 0x10);[m
[31m-[m
[31m-                offset += header.subpacketSize;[m
[31m-            }[m
[31m-[m
[31m-        }[m
[31m-[m
[31m-        public static unsafe void DecryptPacket(Blowfish blowfish, ref BasePacket packet)[m
[31m-        {[m
[31m-            byte[] data = packet.data;[m
[31m-            int size = packet.header.packetSize;[m
[31m-[m
[31m-            int offset = 0;[m
[31m-            while (offset < data.Length)[m
[31m-            {[m
[31m-                if (data.Length < offset + SubPacket.SUBPACKET_SIZE)[m
[31m-                    throw new OverflowException("Packet Error: Subpacket was too small");[m
[31m-[m
[31m-                SubPacketHeader header;[m
[31m-                fixed (byte* pdata = &data[offset])[m
[31m-                {[m
[31m-                    header = (SubPacketHeader)Marshal.PtrToStructure(new IntPtr(pdata), typeof(SubPacketHeader));[m
[31m-                }[m
[31m-[m
[31m-                if (data.Length < offset + header.subpacketSize)[m
[31m-                    throw new OverflowException("Packet Error: Subpacket size didn't equal subpacket data");[m
[31m-[m
[31m-                blowfish.Decipher(data, offset + 0x10, header.subpacketSize - 0x10);[m
[31m-[m
[31m-                offset += header.subpacketSize;[m
[31m-            }[m
[31m-        }[m
[31m-        #endregion[m
[31m-[m
[31m-        public void debugPrintPacket()[m
[31m-        {[m
[31m-#if DEBUG[m
[31m-            Console.BackgroundColor = ConsoleColor.DarkYellow;[m
[31m-            Console.WriteLine("IsAuthed: {0}, IsEncrypted: {1}, Size: 0x{2:X}, Num Subpackets: {3}", header.isAuthenticated, header.isEncrypted, header.packetSize, header.numSubpackets);[m
[31m-            foreach (SubPacket sub in GetSubpackets())[m
[31m-                sub.debugPrintSubPacket();[m
[31m-            Console.BackgroundColor = ConsoleColor.Black;[m
[31m-#endif[m
[31m-        }[m
[31m-[m
[31m-    }[m
[31m-}[m
\ No newline at end of file[m
[1mdiff --git a/Server/MMOServer/MMOServer/Blowfish.cs b/Server/MMOServer/MMOServer/Blowfish.cs[m
[1mdeleted file mode 100644[m
[1mindex 699ec47..0000000[m
[1m--- a/Server/MMOServer/MMOServer/Blowfish.cs[m
[1m+++ /dev/null[m
[36m@@ -1,459 +0,0 @@[m
[31m-﻿using System;[m
[31m-[m
[31m-namespace MMOServer[m
[31m-{[m
[31m-    public class Blowfish[m
[31m-    {[m
[31m-        const int N = 16;[m
[31m-        uint[] P = new uint[16 + 2];[m
[31m-        uint[,] S = new uint[4, 256];[m
[31m-[m
[31m-        #region P and S Values[m
[31m-[m
[31m-        byte[] P_values =[m
[31m-        {[m
[31m-            0x88, 0x6A, 0x3F, 0x24, 0xD3, 0x08, 0xA3, 0x85, 0x2E, 0x8A, 0x19, 0x13, 0x44, 0x73, 0x70, 0x03,[m
[31m-            0x22, 0x38, 0x09, 0xA4, 0xD0, 0x31, 0x9F, 0x29, 0x98, 0xFA, 0x2E, 0x08, 0x89, 0x6C, 0x4E, 0xEC,[m
[31m-            0xE6, 0x21, 0x28, 0x45, 0x77, 0x13, 0xD0, 0x38, 0xCF, 0x66, 0x54, 0xBE, 0x6C, 0x0C, 0xE9, 0x34,[m
[31m-            0xB7, 0x29, 0xAC, 0xC0, 0xDD, 0x50, 0x7C, 0xC9, 0xB5, 0xD5, 0x84, 0x3F, 0x17, 0x09, 0x47, 0xB5,[m
[31m-            0xD9, 0xD5, 0x16, 0x92, 0x1B, 0xFB, 0x79, 0x89[m
[31m-        };[m
[31m-[m
[31m-        byte[] S_values =[m
[31m-        {[m
[31m-            0xA6, 0x0B, 0x31, 0xD1, 0xAC, 0xB5, 0xDF, 0x98, 0xDB, 0x72, 0xFD, 0x2F, 0xB7, 0xDF, 0x1A, 0xD0,[m
[31m-            0xED, 0xAF, 0xE1, 0xB8, 0x96, 0x7E, 0x26, 0x6A, 0x45, 0x90, 0x7C, 0xBA, 0x99, 0x7F, 0x2C, 0xF1,[m
[31m-            0x47, 0x99, 0xA1, 0x24, 0xF7, 0x6C, 0x91, 0xB3, 0xE2, 0xF2, 0x01, 0x08, 0x16, 0xFC, 0x8E, 0x85,[m
[31m-            0xD8, 0x20, 0x69, 0x63, 0x69, 0x4E, 0x57, 0x71, 0xA3, 0xFE, 0x58, 0xA4, 0x7E, 0x3D, 0x93, 0xF4,[m
[31m-            0x8F, 0x74, 0x95, 0x0D, 0x58, 0xB6, 0x8E, 0x72, 0x58, 0xCD, 0x8B, 0x71, 0xEE, 0x4A, 0x15, 0x82,[m
[31m-            0x1D, 0xA4, 0x54, 0x7B, 0xB5, 0x59, 0x5A, 0xC2, 0x39, 0xD5, 0x30, 0x9C, 0x13, 0x60, 0xF2, 0x2A,[m
[31m-            0x23, 0xB0, 0xD1, 0xC5, 0xF0, 0x85, 0x60, 0x28, 0x18, 0x79, 0x41, 0xCA, 0xEF, 0x38, 0xDB, 0xB8,[m
[31