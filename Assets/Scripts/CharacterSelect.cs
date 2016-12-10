using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour {
    public GameObject cursor;
    public GameObject selectedCharacter;
    private GameObject selectedGameObject;
    private Text selectedText;
    private CursorMover cm;
    public static ushort selectedSlot;
    private bool allowedToCreateCharacter;
    
    //TODO: Refuse character creation if character already created on that slot

    void Start()
    {
        allowedToCreateCharacter = false;
        cm = cursor.GetComponent<CursorMover>();
        selectedGameObject = cm.GetSelectedOption();
        selectedCharacter = selectedGameObject;
        selectedText = selectedGameObject.GetComponent<Text>();
    //    selectedSlot = 0;
    }
	// Update is called once per frame
	void Update () {

        selectedText.color = Color.red;
        selectedGameObject = cm.menuObjects[selectedSlot];
        if (CharacterInSlot(selectedSlot) && IsASlotItem())
        {
            cm.menuObjects[4].GetComponent<Text>().color = Color.white;
            cm.menuObjects[3].GetComponent<Text>().color = Color.grey;
            //Set enter to valid 
            //Set create character invalid
            allowedToCreateCharacter = false;
        }
        else if (IsASlotItem())
        {
            cm.menuObjects[4].GetComponent<Text>().color = Color.grey;
            cm.menuObjects[3].GetComponent<Text>().color = Color.white;
            allowedToCreateCharacter = true;
            //set enter to invalid
            //Set create character to valid
        }

        if (Input.GetButtonDown("Fire1"))
        {
            var refToPreviousSlot = selectedGameObject;
            selectedGameObject = cm.GetSelectedOption();
            if (selectedGameObject == cm.menuObjects[3] && allowedToCreateCharacter && cursor.activeInHierarchy) //3 is Create
            {
                selectedGameObject.gameObject.GetComponent<CharacterMenuPrefabHandler>().InstantiatePrefabAsChangedMenu(MenuPrefabs.CharacterCreate);
                selectedGameObject = refToPreviousSlot;
                SetCharacterSlot();
            }
            else
            if (selectedGameObject.transform.IsChildOf(transform) && IsASlotItem())
            {
                
                selectedText.color = Color.white;
                selectedText = selectedGameObject.GetComponent<Text>();
                selectedCharacter = selectedGameObject;
                selectedText.color = Color.red;
                SetCharacterSlot();
            }

        }
        allowedToCreateCharacter = false;
    }

    private bool CharacterInSlot(ushort selectedSlot)
    {
        var blah = GameObject.FindGameObjectWithTag("Slot" + (selectedSlot+1));
        foreach (Transform child in blah.transform)
        {
            if (child.CompareTag("Character") && selectedGameObject == cm.menuObjects[selectedSlot])
            {
                return true;
            }
        }

        
        return false;
    }

    private void SetCharacterSlot()
    {
        if (selectedCharacter == cm.menuObjects[0])
        {
            selectedSlot = 0;
        }
        if (selectedCharacter == cm.menuObjects[1])
        {
            selectedSlot = 1;
        }
        if (selectedCharacter == cm.menuObjects[2])
        {
            selectedSlot = 2;
        }
    }

    private bool IsASlotItem()
    {
        return ((selectedGameObject != cm.menuObjects[3]) && (selectedGameObject != cm.menuObjects[4]) && (selectedGameObject != cm.menuObjects[5]));
    }

    public GameObject GetSelectedCharacter()
    {
        return selectedCharacter;
    }
}
