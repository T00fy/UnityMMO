using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class CharacterDestroyer : MonoBehaviour
{

    private Queue<Character> disconnectionQueue = new Queue<Character>();
    

    // Update is called once per frame
    void Update()
    {
        if (disconnectionQueue.Count > 0)
        {
            var player = disconnectionQueue.Dequeue().gameObject;
            Destroy(player);
        }
    }

    public void AddCharacter(Character playerToDisconnect)
    {
        disconnectionQueue.Enqueue(playerToDisconnect);
    }
}
