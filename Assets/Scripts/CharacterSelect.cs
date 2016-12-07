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
    
    //TODO: Refuse character creation if character already created on that slot

    void Start()
    {
        cm = cursor.GetComponent<CursorMover>();
        selectedGameObject = cm.GetSelectedOption();
        selectedCharacter = selectedGameObject;
        selectedText = selectedGameObject.GetComponent<Text>();
        
    }
	// Update is called once per frame
	void Update () {

        selectedText.color = Color.red;
        if (Input.GetButtonDown("Fire1"))
        {
            selectedGameObject = cm.GetSelectedOption();
            if (selectedGameObject.transform.IsChildOf(transform) && NotAMenuAction())
            {
                
                selectedText.color = Color.white;
                selectedText = selectedGameObject.GetComponent<Text>();
                selectedCharacter = selectedGameObject;
                selectedText.color = Color.red;
                SetCharacterSlot();
            }

            if (selectedGameObject == cm.menuObjects[3]) //3 is Create
            {
                selectedGameObject.gameObject.GetComponent<CharacterMenuPrefabHandler>().InstantiatePrefabAsChangedMenu(MenuPrefabs.CharacterCreate);
            }

        }

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

    private bool NotAMenuAction()
    {
        return ((selectedGameObject != cm.menuObjects[3]) && (selectedGameObject != cm.menuObjects[4]) && (selectedGameObject != cm.menuObjects[5]));
    }

    public GameObject GetSelectedCharacter()
    {
        return selectedCharacter;
    }
}
