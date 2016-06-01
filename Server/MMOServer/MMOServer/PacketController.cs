using System;
using System.Collections.Generic;

namespace MMOServer
{
    /***
     * 
     * Packet protocol:
     * 
     * Max overall packet length is 1024 bytes. 1012 for the payload
     * 
     * First 4 bytes is reserved for length of payload (max of 1012 bytes)
     * 
     * second 4 bytes reserved for int enum denoting the type of the packet being sent
     * 
     * third 4 bytes reserved for boolean that states to continue reading after first packet sent 
     * (wasteful but compiler stores booleans as a byte in memory, could possibly make this better in future)
     * 
     * Rest of packet is the payload
     * 
     */ 

    //packet gets checked first for whether or not it is fragmented
    //if it is fragmented, read all of the first 1012 buffer bytes and append to a stringbuilder/whatever depending on enum type
    //continue until you do not receive a packet with a continue byte
    // when you do, follow the normal process (allocate a buffer array depending on payload length)

        //potential problem: might result in potential merging of byte array depending on what source is. Might not have to worry about this.


    //last return should be IList ArraySegment, with 4 segments as outlined above


    class PacketController
    {
        private PacketTypes type;
        private bool fragment;
        private byte[] data;
        private int dataLength;
        private IList<ArraySegment<byte>> finalPacket; 


        public PacketController(PacketTypes type, byte[] data)
        {
            this.type = type;

            if (data.Length > 1012)
            {
                fragment = true;
            }
            else
            {
                fragment = false;
            }
            this.data = data;
            dataLength = data.Length;

        }


        //will return true if
        public bool InitializePacket()
        {

            throw new NotImplementedException();

        }

        public PacketController(byte[] data)
        {
            
            //use default values here
        }

        public byte[] GetPacketToSend()
        {
            throw new NotImplementedException();
        }

        public PacketTypes GetPacketType()
        {
            return type;
        }




    }
}
