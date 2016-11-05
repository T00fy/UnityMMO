using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour {
    public GameObject cursor;
    public GameObject selectedCharacter;
    private GameObject selectedGameObject;
    private Text selectedText;
    private CursorMover cm;
    

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
            if (selectedGameObject.transform.IsChildOf(transform) && selectedGameObject != cm.menuObjects[3])
            {
                selectedText.color = Color.white;
                selectedText = selectedGameObject.GetComponent<Text>();
                selectedCharacter = selectedGameObject;
                selectedText.color = Color.red;
            }

            if (selectedGameObject == cm.menuObjects[3]) //3 is Create
            {
                selectedGameObject.gameObject.GetComponent<CharacterMenuPrefabHandler>().InstantiatePrefabAsChangedMenu(MenuPrefabs.CharacterCreate);
            }

        }

    }

    public GameObject GetSelectedCharacter()
    {
        return selectedCharacter;
    }
}
