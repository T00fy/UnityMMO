using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterCreateHandler : MonoBehaviour {

    public CharacterSelect characterSelect;
    public GameObject[] statNumbers;
    public GameObject nameField;
    private GameObject selectedSlot;
    private bool statSelected;
	// Use this for initialization
	void Start () {
        selectedSlot = characterSelect.GetSelectedCharacter();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Fire1"))
        {
            var selectedOption = gameObject.GetComponent<CursorMover>().GetSelectedOption();

            if (selectedOption != nameField) //statnumbers 5 is inputfield
            {
                statSelected = true;
                selectedOption.GetComponent<Text>().color = Color.red;
            }
        }

        //check if character slot is empty
        //
    }
}
