using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMOServer
{
    public enum SubPacketTypes
    {
        Account,GamePacket,DBQuery
    }

    public enum BasePacketConnectionTypes
    {
        Zone,Chat
    }

    public enum GamePacketOpCode
    {
        Error,Success
    }

    public enum ErrorCodes
    {
        NoAccount,WrongPassword,DuplicateAccount
    }
}
