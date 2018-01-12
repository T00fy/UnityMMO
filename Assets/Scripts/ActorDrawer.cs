using MMOServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class ActorDrawer : MonoBehaviour
{

    public Camera cam;
    public float howOftenToCheckForNearbyCharacters = 0.1f;
    private Connection connection;
    private Queue<ActorWrapper> actorsToDraw = new Queue<ActorWrapper>();
    Sprite[] sprites;

    // Use this for initialization
    void Start()
    {
        GameEventManager.ActorNeedsDrawing += new GameEventManager.GameEvent(AddToDrawQueue);
        connection = GameObject.Find("WorldServerConnection").GetComponent<Connection>();
        sprites = Resources.LoadAll<Sprite>("Sprite stuffnobg");
        InvokeRepeating("QueryForNearybyActors", 0.0f, howOftenToCheckForNearbyCharacters);

    }

    private void QueryForNearybyActors()
    {
        var bounds = cam.OrthographicBounds();
        PositionsInBoundsPacket packet = new PositionsInBoundsPacket(bounds.min.x, bounds.max.x, bounds.min.y, bounds.max.y);
        Debug.Log(bounds.min.x + "," + bounds.min.y + "," + bounds.max.x + "," + bounds.max.y);
        SubPacket sp = new SubPacket(GamePacketOpCode.NearbyActorsQuery, Data.CHARACTER_ID, 0, packet.GetBytes(), SubPacketTypes.GamePacket);
        Debug.Log(PacketProcessor.isAuthenticated);
        connection.Send(BasePacket.CreatePacket(sp, PacketProcessor.isAuthenticated, false));
    }

    private void AddToDrawQueue(GameEventArgs eventArgs)
    {
        ActorWrapper actor = eventArgs.Actor;
        Debug.Log("Adding actor to draw queue: " + actor.Id);
        actorsToDraw.Enqueue(actor);
    }

    // Update is called once per frame
    void Update()
    {
        if (actorsToDraw.Count > 0)
        {
            ActorWrapper actorToDraw = actorsToDraw.Dequeue();
            GameObject obj; //has to be handled on the main thread

            if (actorToDraw.Playable)
            {
                Debug.Log("Actor is a character");
                obj = GetDrawnActor(Data.drawnCharacters, actorToDraw);
            }
            else
            {
                Debug.Log("Actor is npc");
                obj = GetDrawnActor(Data.drawnNpcs, actorToDraw);
            }
            obj.transform.position.Set(actorToDraw.XPos, actorToDraw.YPos, 0.0f);

        }
        //Poll for nearby actors every x seconds
        //add drawn actor to a list
        //When character is out of bounds of the camera, set character gameobject to inactive
        //When charcter enters bounds again, look up list first and set gameobject to active, if not there create them
        //Poll periodically for the position of characters within the camera bounds and update their positions.
        //Create some sort of AI that when given a new position from a current position, the character will animate movement to that location
    }

    private GameObject GetDrawnActor<T>(Dictionary<uint, T> drawnActors, ActorWrapper actorToDraw) where T : Actor
    {
        if (drawnActors.ContainsKey(actorToDraw.Id))
        {
            Debug.Log("Actor is already within bounds!");
            T actor;
            drawnActors.TryGetValue(actorToDraw.Id, out actor);
            GameObject obj = actor.gameObject;
            obj.SetActive(true);
            return obj;
        }
        else
        {
            Debug.Log("Creating new Actor object!");
            GameObject obj = new GameObject("Actor");
            var actor = obj.AddComponent<T>();
            actor.Id = actorToDraw.Id;
            drawnActors.Add(actor.Id, actor);
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprites[0];
            spriteRenderer.sortingOrder = 1;
            return obj;
        }
    }
}
