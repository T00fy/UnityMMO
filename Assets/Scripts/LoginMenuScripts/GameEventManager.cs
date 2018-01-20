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
    public static GameEvent PollerPositionPacket;

    public static event GameEvent ActorNeedsDrawing;

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

    public static void TriggerActorNeedsDrawing(GameEventArgs eventArgs = null)
    {
        if (ActorNeedsDrawing != null)
        {
            ActorNeedsDrawing(eventArgs);
        }
    }

    public static void TriggerPollerResponse(GameEventArgs eventArgs = null)
    {
        if (PollerPositionPacket != null)
        {
            PollerPositionPacket(eventArgs);
        }
    }

}

