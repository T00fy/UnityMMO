using UnityEngine;
using System.Collections;
using MMOServer;
using System.Collections.Generic;
using System;

public class CharacterLoader : MonoBehaviour {
    public StatusBoxHandler statusBoxHandler;
    public List<SubPacket> characterServerResponse;
    public static bool serverResponseFinished = false;
    private Character[] characters;
    private PacketProcessor packetProcessor;
    public Sprite characterModel;
    public GameObject slot1;
    public GameObject slot2;
    public GameObject slot3;


    // Use this for initialization
    void OnEnable()
    {
        characterServerResponse = new List<SubPacket>();
        CharacterQueryPacket cq = new CharacterQueryPacket(Utils.GetAccountName());
        SubPacket sp = cq.BuildQueryPacket();
        BasePacket packetToSend = BasePacket.CreatePacket(sp, PacketProcessor.isAuthenticated, false);
        packetProcessor = GameObject.FindGameObjectWithTag("PacketProcessor").GetComponent<PacketProcessor>();
        statusBoxHandler.InstantiateStatusBoxPrefabWithNoMenuLink(MenuPrefabs.StatusBox);
        StatusBoxHandler.statusText = "Status: Retrieving character list from server";

        packetProcessor.SendPacket(packetToSend);
        StartCoroutine(WaitForServerResponseThenDisplayCharacters());
        //send character query packet to server
        //open status box you cant close until received response from server
        //receive what characters are avaiable for this account if any
        //draw them on character select boxes
    }
    private IEnumerator WaitForServerResponseThenDisplayCharacters()
    {
        while (!serverResponseFinished)
        {
            yield return null;
        }

        if (characterServerResponse.Count > 0)
        {

            //load character here
            //will have to change/add a lot more later to this when you can edit race/face/etc
            StatusBoxHandler.statusText = "Status: Loading characters";
            Debug.Log("Amount of characters found: " + characterServerResponse.Count);
            for (int i = 0; i < characterServerResponse.Count; i++)
            {
                CharacterQueryPacket cq = new CharacterQueryPacket();
                cq.ReadResponsePacket(characterServerResponse[i]);
                GameObject characterHolder = GetCharacterModelHolder(cq.GetCharacterSlot());
                if (!CharacterModelAlreadyLoaded(characterHolder))
                { 
                    GameObject characterSprite = new GameObject();
                    Character character = characterSprite.AddComponent<Character>();
                    character.SetCharacterInfoFromPacket(cq);
                    LoadCharacterInSlot(characterSprite, characterHolder);
                }
            }
        }
        statusBoxHandler.DestroyStatusBox();
    }

    private GameObject GetCharacterModelHolder(ushort v)
    {
        switch (v)
        {
            case 0:
                return slot1;
            case 1:
                return slot2;
            case 2:
                return slot3;
        }
        throw new Exception("There should only be 3 slots");
    }

    void OnDisable()
    {
        serverResponseFinished = false;
        CleanUpCharacterSlots();
        
    }

    //have to do it this hori way because unity is fucking quirky
    private void CleanUpCharacterSlots()
    {
        
        if(slot1.transform.childCount > 0)
            Destroy(slot1.transform.GetChild(0).gameObject);
        if (slot2.transform.childCount > 0)
            Destroy(slot2.transform.GetChild(0).gameObject);
        if (slot3.transform.childCount > 0)
            Destroy(slot3.transform.GetChild(0).gameObject);
    }

    private bool CharacterModelAlreadyLoaded(GameObject characterHolder)
    {
        if (characterHolder.transform.childCount > 0)
        {
            return true;
        }
        return false;
    }

    //this currently will only ever set it to a sprite defined in editor. Will need to change this in future
    //when different types of character designs are added.
    private void LoadCharacterInSlot(GameObject characterSprite, GameObject characterHolder)
    {
        characterSprite.name = characterSprite.GetComponent<Character>().CharacterName;
        Debug.Log(characterSprite.GetComponent<Character>().CharacterName);
        characterSprite.transform.SetParent(characterHolder.transform);
        characterSprite.transform.localPosition = Vector3.zero;
        SpriteRenderer spriteRenderer = characterSprite.AddComponent<SpriteRenderer>();
        characterSprite.tag = "Character";
        spriteRenderer.sprite = characterModel;
    }

    public void SetCharacterListFromServer(SubPacket response)
    {
        characterServerResponse.Add(response);
    }
}
