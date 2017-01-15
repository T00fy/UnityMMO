using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public static class GameEventManager
{
    

    public delegate void GameEvent(GameEventArgs eventArgs);
    public static event GameEvent HandshakeResponse;
    public static event GameEvent StatusBoxClosed;
    public static GameEvent ClientWantsToEnter;

    public static void TriggerHandshakeResponseReceived(GameEventArgs eventArgs = null)
    {
        if (HandshakeResponse != null)
        {
            HandshakeResponse(eventArgs);
        }
    }

    public static void TriggerStatusBoxClosed(GameEventArgs eventArgs = null)
    {
        if (StatusBoxClosed != null)
        {
            StatusBoxClosed(eventArgs);
        }
    }

    public static void TriggerClientWantsToEnter(GameEventArgs eventArgs = null)
    {
        if (ClientWantsToEnter != null)
        {
            ClientWantsToEnter(eventArgs);
        }
    }
}

