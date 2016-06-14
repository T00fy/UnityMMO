using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMOServer
{
    class ErrorPacket
    {
        private string errorMessage;
        private uint errorId;


        public ErrorPacket(ErrorCodes errorId, string message)
        {
            errorMessage = message;
            this.errorId = (uint)errorId;
        }

        public SubPacket buildPacket()
        {
            MemoryStream memStream = new MemoryStream();
            BinaryWriter binWriter = new BinaryWriter(memStream);

            binWriter.Write(errorId);
            binWriter.Write(errorMessage);

            byte[] data = memStream.GetBuffer();
            memStream.Dispose();
            binWriter.Dispose();
            SubPacket subPacket = new SubPacket(GamePacketOpCode.Error, 0, 0, data, SubPacketTypes.GamePacket);
            return subPacket;
        }
    }
}
