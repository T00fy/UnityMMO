using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    class ClientConnect
    {
        public Blowfish blowfish;
        public Socket socket;
        public byte[] buffer = new byte[0xffff];
        public CircularBuffer<byte> incomingStream = new CircularBuffer<byte>(1024);
        public BlockingCollection<BasePacket> sendPacketQueue = new BlockingCollection<BasePacket>(100);
        public int lastPartialSize = 0;
    }
}
