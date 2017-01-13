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
        Zone,Chat,Generic
    }

    public enum GamePacketOpCode
    {
        AccountError,AccountSuccess,CreateCharacter,RegisterSuccess,CreateCharacterError,CreateCharacterSuccess,CharacterListQuery,CharacterDeleteQuery,CharacterDeleteSuccess,
        Handshake, Acknowledgement
    }

    public enum ErrorCodes
    {
        NoAccount,WrongPassword,DuplicateAccount,StatsNotUsed,DuplicateCharacter,UnknownDatabaseError, CharacterDeleteError
    }
}
