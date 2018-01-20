using MMOServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class ActorDrawer : MonoBehaviour
{

    public Camera cam;
    public GameObject actorHolder; //just an empty gameobject container
    public float howOftenToCheckForNearbyCharacters = 1.0f;
    public float howOftenToCheckToSetInactive = 1.0f;
    private Connection connection;
    private Queue<ActorWrapper> actorsToDraw = new Queue<ActorWrapper>();
    private Sprite[] sprites;
    private Bounds cameraBounds;

    // Use this for initialization
    void Start()
    {
        GameEventManager.ActorNeedsDrawing += new GameEventManager.GameEvent(AddToDrawQueue);
        connection = GameObject.Find("WorldServerConnection").GetComponent<Connection>();
        cameraBounds = cam.OrthographicBounds();
        sprites = Resources.LoadAll<Sprite>("Sprite stuffnobg");
        InvokeRepeating("QueryForNearybyActors", 0.0f, howOftenToCheckForNearbyCharacters);
        StartCoroutine(CheckInactiveCharacters(Data.drawnCharacters));
    }

    private IEnumerator CheckInactiveCharacters(Dictionary<uint, Character> drawnCharacters)
    {
        while (true)
        {
            SetInactiveOutOfBoundsActors(drawnCharacters);
            yield return new WaitForSeconds(howOftenToCheckToSetInactive);
        }
    }

    private void QueryForNearybyActors()
    {
        PositionsInBoundsPacket packet = new PositionsInBoundsPacket(cameraBounds.min.x, cameraBounds.max.x, cameraBounds.min.y, cameraBounds.max.y);
        Debug.Log(cameraBounds.min.x + "," + cameraBounds.min.y + "," + cameraBounds.max.x + "," + cameraBounds.max.y);
        SubPacket sp = new SubPacket(GamePacketOpCode.NearbyActorsQuery, Data.CHARACTER_ID, 0, packet.GetBytes(), SubPacketTypes.GamePacket);
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
        cameraBounds = cam.OrthographicBounds();
        if (actorsToDraw.Count > 0)
        {

            ActorWrapper actorToDraw = actorsToDraw.Dequeue();
            if (actorToDraw.Id == Data.CHARACTER_ID)
            { 
                throw new Exception("WTF!!! Why does it keep getting here");
            }
            GameObject obj; //has to be handled on the main thread

            if (actorToDraw.Playable)
            {
                Debug.Log("Actor is a character");
                obj = GetDrawnActor(Data.drawnCharacters, actorToDraw);
                if (obj != null)
                {
                    HandleNewActorSetup(obj, actorToDraw.XPos, actorToDraw.YPos);
                }

            }
            else
            {
                Debug.Log("Actor is npc");
                obj = GetDrawnActor(Data.drawnNpcs, actorToDraw);
            }

        }
        //Poll for nearby actors every x seconds
        //add drawn actor to a list
        //When character is out of bounds of the camera, set character gameobject to inactive
        //When charcter enters bounds again, look up list first and set gameobject to active, if not there create them
        //Poll periodically for the position of characters within the camera bounds and update their positions.
        //Create some sort of AI that when given a new position from a current position, the character will animate movement to that location
    }

    /// <summary>
    /// Adds all the necessary script components for a new Character
    /// </summary>
    /// <param name="obj">The GameObject with the Actor script</param>
    /// <param name="xPos">The starting xPos of the Actor</param>
    /// <param name="yPos">The starting yPos of the Actor</param>
    private void HandleNewActorSetup(GameObject actor, float xPos, float yPos)
    {
        actor.transform.position.Set(xPos, yPos, 0.0f);
        actor.AddComponent<CharacterMovement>();
        actor.AddComponent<CharacterPositionPoller>();
        SpriteRenderer spriteRenderer = actor.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[0];
        spriteRenderer.sortingOrder = 1;

        Rigidbody2D rb = actor.GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        Animator animator = actor.GetComponent<Animator>();

        animator.runtimeAnimatorController = Resources.Load("Assets/Resources/Animation/player.controller") as RuntimeAnimatorController;

    }

    private void SetInactiveOutOfBoundsActors<T>(Dictionary<uint, T> drawnActors) where T : Actor
    {
        foreach (var actor in drawnActors)
        {
            if (actor.Value.gameObject.activeInHierarchy)
            {
                Debug.Log("extents: "+cameraBounds.ClosestPoint(Vector3.zero));
                Debug.Log("actor pos: " + actor.Value.transform.position);
                if (!cam.ActorOnScreen(actor.Value.transform))
                {
                    actor.Value.gameObject.SetActive(false);
                }
            }
        }
    }


    /// <summary>
    /// Gets a GameObject from an Actor derived dictionary. 
    /// If it doesn't exist will create a new gameobject and add it to the dictionary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="drawnActors"></param>
    /// <param name="actorToDraw"></param>
    /// <returns></returns>
    private GameObject GetDrawnActor<T>(Dictionary<uint, T> drawnActors, ActorWrapper actorToDraw) where T : Actor
    {
        if (drawnActors.ContainsKey(actorToDraw.Id))
        {
            Debug.Log("Actor is already within bounds!");
            T actor;
            drawnActors.TryGetValue(actorToDraw.Id, out actor);
            GameObject obj = actor.gameObject;
            obj.SetActive(true);
            return null;
        }
        else
        {
            Debug.Log("Creating new Actor object!");
            GameObject obj = new GameObject("Actor");
            obj.transform.parent = actorHolder.transform;
            var actor = obj.AddComponent<T>();
            actor.Id = actorToDraw.Id;
            drawnActors.Add(actor.Id, actor);
            return obj;
        }
    }
}
