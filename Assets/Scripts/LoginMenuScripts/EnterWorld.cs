using MMOServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnterWorld : MonoBehaviour
{
    public Connection worldServerConnection;
    public Connection loginServerConnection;
    public MenuPrefabHandler genericBoxHandler;
    public StatusBoxHandler statusBoxHandler;
    private bool? handShakeSuccessful;
    private bool clientActivatedEnterWorld;
    private const float TIMEOUT_DURATION = 30.0f;
    private float countDown;
    private CharacterSelect characterSelect;

    // Use this for initialization
    void Start()
    {
        GameEventManager.HandshakeResponse += new GameEventManager.GameEvent(ServerResponse);
        GameEventManager.ClientWantsToEnter += new GameEventManager.GameEvent(InputFromCharacterSelect);
        GameEventManager.StatusBoxClosed += new GameEventManager.GameEvent(StatusBoxResponse);
        countDown = TIMEOUT_DURATION;
        DontDestroyOnLoad(worldServerConnection);
    }

    private void ServerResponse(GameEventArgs eventArgs)
    {
        handShakeSuccessful = eventArgs.serverResponse;
    }

    private void InputFromCharacterSelect(GameEventArgs eventArgs)
    {
        clientActivatedEnterWorld = eventArgs.clientSelectedEnterWorld;
        var selectGameObject = Utils.FindSiblingGameObjectByName(gameObject, "CharacterSlots");
        characterSelect = selectGameObject.GetComponent<CharacterSelect>();
        characterSelect.enabled = false;

    }

    private void StatusBoxResponse(GameEventArgs eventArgs)
    {
        try
        {
            characterSelect.enabled = true;
        }
        catch (NullReferenceException) { } //this is a hack until I change all instances of 'statusboxclosed' boolean to using an event
        
    }

    // Update is called once per frame
    void Update()
    {
        if (clientActivatedEnterWorld) //if is entering
        {
            worldServerConnection.EstablishConnection("127.0.0.1", 3435);
            int characterId = Utils.GetCharacter(CharacterSelect.selectedSlot).CharId;
            var characterIdBytes = BitConverter.GetBytes(characterId);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(characterIdBytes);
            }
            SubPacket packetToSend = new SubPacket(GamePacketOpCode.Handshake, 0, 0, characterIdBytes, SubPacketTypes.GamePacket);
            BasePacket test = BasePacket.CreatePacket(packetToSend, PacketProcessor.isAuthenticated, false);
            test.header.connectionType = (ushort)BasePacketConnectionTypes.Connect;
            worldServerConnection.Send(test);
            genericBoxHandler.InstantiateMessageOnlyStatusBox();
            var boxText = genericBoxHandler.GetPrefab().GetComponentInChildren<Text>();
            boxText.text = "Handshaking with server..";

            StartCoroutine(WaitForServerResponse());
            clientActivatedEnterWorld = false;
        }
    }

    private IEnumerator WaitForServerResponse()
    {
        while (!handShakeSuccessful.HasValue)
        {
            countDown -= Time.deltaTime;
            if (countDown < 0)
            {
                break;
            }
            yield return null;
        }
        countDown = TIMEOUT_DURATION; //reset coutdown;
        if (handShakeSuccessful.HasValue)
        {
            if ((bool)handShakeSuccessful)
            {
                loginServerConnection.SendDisconnectPacket();
                SceneManager.LoadScene("test", LoadSceneMode.Single);
                //load world instantly
            }
            else
            {
                Debug.Log("Received response but couldn't authenticate you with login server");
                //make status box with message saying something messed up
            }
        }
        else
        {
            genericBoxHandler.DestroyMessageOnlyStatusBox();
            statusBoxHandler.InstantiatePrefab(MenuPrefabs.StatusBox, "Connection to server timed out");
        }


    }
}
