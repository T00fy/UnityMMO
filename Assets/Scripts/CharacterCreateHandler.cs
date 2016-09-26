using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class CharacterCreateHandler : MonoBehaviour {

    public CharacterSelect characterSelect;
    public GameObject[] statNumbers;
    public GameObject nameField;
    public GameObject statsLeft;

    private GameObject selectedSlot;
    private bool statSelected;
    private Animator animator;
    private CursorInput cursorInput;
    private CursorMover cm;
    private int statCounter;
    private bool reachedMaxStats;


	// Use this for initialization
	void Start () {
        reachedMaxStats = false;
        selectedSlot = characterSelect.GetSelectedCharacter();
        animator = gameObject.GetComponent<Animator>();
        cursorInput = gameObject.GetComponent<CursorInput>();
        cm = gameObject.GetComponent<CursorMover>();
        animator.enabled = false;
        statSelected = false;
        statCounter = int.Parse(statsLeft.GetComponent<Text>().text);

    }
	
	// Update is called once per frame
	void Update () {
        if (statCounter < 1)
        {
            reachedMaxStats = true;
        }
        else
        {
            reachedMaxStats = false;
        }

        var selectedOption = gameObject.GetComponent<CursorMover>().GetSelectedOption();
        if (Input.GetButtonDown("Fire1") && !statSelected)
        {
            if (selectedOption != nameField) 
            {
                cursorInput.enabled = false;
                cm.enabled = false;
                animator.enabled = true;
                statSelected = true;
                selectedOption.GetComponent<Text>().color = Color.red;
                selectedOption.transform.GetChild(0).GetComponent<Text>().color = Color.red;
            }
        }
        else if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2") &&  statSelected)
        {
            selectedOption.GetComponent<Text>().color = Color.white;
            selectedOption.transform.GetChild(0).GetComponent<Text>().color = Color.white;
            cursorInput.enabled = true;
            cm.enabled = true;
            animator.enabled = false;
            statSelected = false;
        }
        if (Input.GetKeyDown("right") && statSelected && !reachedMaxStats)
        {
            string rawNumber = selectedOption.transform.GetChild(0).GetComponent<Text>().text;
            int convertedNumber = DoStatChange(rawNumber, 1);
            selectedOption.transform.GetChild(0).GetComponent<Text>().text = convertedNumber.ToString();
        }

        if (Input.GetKeyDown("left") && statSelected)
        {
            string rawNumber = selectedOption.transform.GetChild(0).GetComponent<Text>().text;
            int convertedNumber = DoStatChange(rawNumber, -1);
            selectedOption.transform.GetChild(0).GetComponent<Text>().text = convertedNumber.ToString();
        }

        statsLeft.GetComponent<Text>().text = statCounter.ToString();
        //check if character slot is empty
        //
    }

    private int DoStatChange(string stringToParse, int numberToAdd)
    {
        int convertedNumber;
        try
        {
            convertedNumber = int.Parse(stringToParse);
            convertedNumber = convertedNumber + numberToAdd;
            
            if (convertedNumber > 9)
            {
                return 9;
            }
            if (convertedNumber < 1)
            {
                return 1;
            }
            statCounter = statCounter - numberToAdd;
            return convertedNumber;
        }
        catch (FormatException)
        {
            Debug.Log("somehow stat wasn't a number");
            return 1;
        }
    }
}
