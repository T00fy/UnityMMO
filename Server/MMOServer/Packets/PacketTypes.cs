using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMOServer
{
    public enum SubPacketTypes
    {
        Account, GamePacket, ErrorPacket
    }

    public enum BasePacketConnectionTypes
    {
        Zone,Chat
    }

    public enum GamePacketOpCode
    {
        AccountError,AccountSuccess,CreateCharacter,RegisterSuccess,CreateCharacterError,CreateCharacterSuccess,CharacterListQuery
    }

    public enum ErrorCodes
    {
        NoAccount,WrongPassword,DuplicateAccount,StatsNotUsed,DuplicateCharacter,UnknownDatabaseError
    }
}
