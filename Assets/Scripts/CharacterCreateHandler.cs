using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using MMOServer;
using System.Text.RegularExpressions;

public class CharacterCreateHandler : MonoBehaviour {

    public GameObject[] statNumbers;
    public GameObject nameField;
    public GameObject statsLeft;
    private ushort totalStatsAllowed;
    private CharacterMenuPrefabHandler characterMenuPrefabHandler;
    private bool statSelected;
    private Animator animator;
    private CursorInput cursorInput;
    private CursorMover cm;
    private int statCounter;
    private bool reachedMaxStats;
    private GameObject handler;
    //need to change this so it gets instantiated as a prefab instead
    //character select should load characters from database

	// Use this for initialization
	void Awake () {
        reachedMaxStats = false;
        //      selectedSlot = characterSelect.GetSelectedCharacter();
        handler = GameObject.Find("StatusBoxHandler");
        characterMenuPrefabHandler = handler.GetComponent<CharacterMenuPrefabHandler>();
        animator = gameObject.GetComponent<Animator>();
        cursorInput = gameObject.GetComponent<CursorInput>();
        cm = gameObject.GetComponent<CursorMover>();
        animator.enabled = false;
        statSelected = false;
        statCounter = int.Parse(statsLeft.GetComponent<Text>().text);
        totalStatsAllowed = (ushort)(statCounter + 5);
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
        if (selectedOption != cm.menuObjects[6])
        {
            if (Input.GetButtonDown("Fire2") && !statSelected)
            {
                characterMenuPrefabHandler.InstantiatePrefab(MenuPrefabs.ModalStatusBox, "Are you sure you want to exit character creation without saving?");
                characterMenuPrefabHandler.HandleExitDecision();
            }else
            if ((Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2")) && statSelected)
            {
                selectedOption.GetComponent<Text>().color = Color.white;
                selectedOption.transform.GetChild(0).GetComponent<Text>().color = Color.white;
                cursorInput.enabled = true;
                cm.enabled = true;
                animator.enabled = false;
                statSelected = false;
            }
            else if (Input.GetButtonDown("Fire1") && !statSelected)
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

        }
        else
        {
            
            if (Input.GetButtonDown("Fire1"))
            {
                if (reachedMaxStats && CharacterNameIsLegal())
                {
                    characterMenuPrefabHandler.InstantiatePrefab(MenuPrefabs.ModalStatusBox, "Are you sure you want to create this character?");
                    characterMenuPrefabHandler.HandleCreateDecision(statNumbers, nameField, totalStatsAllowed);
                }
                else if (!reachedMaxStats)
                {
                    handler.GetComponent<StatusBoxHandler>().InstantiatePrefab(MenuPrefabs.StatusBox, "You need to use all your stats first");

                    //open status box saying you cant do that
                }
                else if (!CharacterNameIsLegal())
                {
                    handler.GetComponent<StatusBoxHandler>().InstantiatePrefab(MenuPrefabs.StatusBox, "Your name needs to have 3-9 alphanumeric characters.");
                }
                
            }
            if (Input.GetButtonDown("Fire2") && !statSelected)
            {
                characterMenuPrefabHandler.CloseAndDiscardCharacterCreateInstance();
                //instantiate modal prefab asking if want to quit character create
            }
        }


    }

    private bool CharacterNameIsLegal()
    {
        Regex r = new Regex("^[a-zA-Z0-9]+$");
        var input = nameField.GetComponent<InputField>().text;
        if (input.Length > 10 || input.Length < 3)
        {
            return false;
        }
        if (r.IsMatch(input))
        {
            return true;
        }
        return false;
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
