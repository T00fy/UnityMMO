using UnityEngine;
using System.Collections;

public class CharacterCreateHandler : MonoBehaviour {

    public CharacterSelect characterSelect;
    public GameObject[] statNumbers;
    public GameObject nameField;
    private GameObject selectedSlot;
	// Use this for initialization
	void Start () {
        selectedSlot = characterSelect.GetSelectedCharacter();
    }
	
	// Update is called once per frame
	void Update () {
        

        //check if character slot is empty
        //
    }
}
