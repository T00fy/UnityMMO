using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public static class GameEventManager
{
    public delegate void GameEvent(GameEventArgs eventArgs);
    public static event GameEvent HandshakeResponse;

    public static void TriggerHandshakeResponseReceived(GameEventArgs eventArgs = null)
    {
        if (HandshakeResponse != null)
        {
            HandshakeResponse(eventArgs);
        }
    }
}

