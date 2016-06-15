using UnityEngine;
using System.Collections;

public class CharacterCreateHandler : MonoBehaviour {

    private CharacterSelect cs;
    private GameObject selectedSlot;
	// Use this for initialization
	void Start () {
        cs = gameObject.transform.parent.GetComponentInChildren<CharacterSelect>();
    }
	
	// Update is called once per frame
	void Update () {
        selectedSlot = cs.GetSelectedCharacter();

        //check if character slot is empty
        //
    }
}
