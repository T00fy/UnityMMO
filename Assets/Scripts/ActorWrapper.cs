using UnityEngine;
using System.Collections;

public class ActorWrapper
{
    public float XPos { get; set; }
    public float YPos { get; set; }
    public bool Playable { get; set; }
    public uint Id { get; set; }

    public ActorWrapper(float xPos, float yPos, bool playable, uint actorId)
    {
        XPos = xPos;
        YPos = yPos;
        Playable = playable;
        Id = actorId;
    }
}
