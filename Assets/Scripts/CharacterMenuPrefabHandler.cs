using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using MMOServer;

public class CharacterMenuPrefabHandler : MenuPrefabHandler {
    private GameObject priorMenu;
    private GameObject characterCreateMenu;
    private bool modalChoiceMade;

    void Update()
    {
        if (modalBoxOpened)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                modalChoiceMade = true;
                Debug.Log("Got here");
                modalChoice = menuHandler.GetCursor().GetComponent<CursorMover>().GetSelectedOption().GetComponent<Text>().text;
                DestroyStatusBox();
            }
            if (Input.GetButtonDown("Fire2"))
            {
                modalChoiceMade = true;
                modalChoice = "No";
               DestroyStatusBox();
            }

        }
    }

    public bool ModalSelectionMade()
    {
        return modalChoiceMade;
    }


    /// <summary>
    /// Instantiates the character create menu
    /// </summary>
    /// <param name="prefabToInstantiate"></param>
    public void InstantiatePrefabAsChangedMenu(MenuPrefabs prefabToInstantiate)
    {
        characterCreateMenu = prefabs[(int)prefabToInstantiate];
        parentCursor = menuHandler.GetCursor();
        menuHandler.ToggleCursor(false);
        characterCreateMenu = Instantiate(characterCreateMenu) as GameObject;
        characterCreateMenu.transform.SetParent(GameObject.Find("MainMenu").transform);
        priorMenu = menuHandler.GetActiveMenu();
        Debug.Log(characterCreateMenu.name);
        priorMenu.SetActive(false);
        menuHandler.AddMenuAsChild(characterCreateMenu);
        menuHandler.SetActiveMenu(characterCreateMenu);
    }

    public void CloseAndDiscardCharacterCreateInstance()
    {
        var temp = menuHandler.GetActiveMenu();
        menuHandler.SetActiveMenu(menus[(int)Menus.CharacterMenu]);
        menuHandler.GetActiveMenu().SetActive(true);
        menuHandler.SetCursor(menuHandler.GetActiveMenu().transform.Find("Cursor").gameObject);
        menuHandler.ToggleCursor(true);
        Debug.Log("gottt");
        if (temp.name == "CharacterCreation(Clone)")
        {
            menuHandler.RemoveChildMenu(characterCreateMenu);
            Destroy(temp);
        }
        else
        {
            temp = GameObject.Find("CharacterCreation(Clone)");
            menuHandler.RemoveChildMenu(temp);
            Destroy(temp);
        }

        
        
    }


    //
    //
    //shameless hack ahead
    //
    //
    public void HandleCreateDecision(GameObject[] statNumbers, GameObject nameField)
    {
        StartCoroutine(WaitForModalCreateDecision(statNumbers, nameField));
        
    }

    public void HandleExitDecision()
    {
        StartCoroutine(WaitForModalExitDecision());

    }

    public IEnumerator WaitForModalExitDecision()
    {
        while (!modalChoiceMade)
        {
            yield return null;
        }
        Debug.Log("Choice made");
        if (AnsweredYes())
        {
            Debug.Log("bruh");
            CloseAndDiscardCharacterCreateInstance();
            modalChoiceMade = false;

        }
        modalChoiceMade = false;
    }

    public IEnumerator WaitForModalCreateDecision(GameObject[] statNumbers, GameObject nameField)
    {
        while (!modalChoiceMade)
        {
            yield return null;
        }
        Debug.Log("Choice made");
        if (AnsweredYes())
        {
            SendCharacterCreateInfo(statNumbers, nameField);
            Debug.Log("bruh");
            CloseAndDiscardCharacterCreateInstance();
            modalChoiceMade = false;

        }
        modalChoiceMade = false;
    }

    private void SendCharacterCreateInfo(GameObject[] statNumbers, GameObject nameField)
    {
        ushort[] stats = new ushort[statNumbers.Length];
        for (int i = 0; i < statNumbers.Length; i++)
        {
            ushort stat = ushort.Parse(statNumbers[i].GetComponent<Text>().text);
            stats[i] = stat;
        }

        //    bytesToSend = o920i
        CharacterPacket cp = new CharacterPacket(nameField.GetComponent<InputField>().text, stats);
        var bytesToSend = cp.GetData();
        SubPacket sp = new SubPacket(GamePacketOpCode.CreateCharacter, 0, 0, bytesToSend, SubPacketTypes.GamePacket);
        BasePacket characterCreationPacket = BasePacket.CreatePacket(sp, PacketProcessor.isAuthenticated, false);

        PacketProcessor.SendCharacterCreationPacket(characterCreationPacket);
    }

    //
    //
    //shameless hack finished
    //
    //when writing this i didn't realise a coroutine is not allowed to be executed from an object that ever becomes deactived, due to
    //the nature of how i implemented the statusboxes and the cursor swapping between being activated and deactived,
    //i had to do it this way. Could write this better in future if I wanted to completely rewrite how prefabs become instantiated
    //but that would take way too long at this point
}
