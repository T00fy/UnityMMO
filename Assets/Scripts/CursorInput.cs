﻿using UnityEngine;
using System;
using UnityEngine.UI;
using MMOServer;

public class CursorInput : MonoBehaviour {
    public static MenuHandler menuHandler;

    private GameObject cursor;
    private GameObject activeMenu;
    private bool enteringText;
    private InputField inputField;
    private CursorMover cm;
    private GameObject selectedOption;

    // Use this for initialization

    void Awake () {
        cursor = gameObject;
        menuHandler = GameObject.Find("MenuHandler").GetComponent<MenuHandler>();
        string parent = cursor.transform.parent.name;
        activeMenu = GameObject.Find(parent);
        menuHandler.SetActiveMenu(activeMenu);
        cm = gameObject.GetComponent<CursorMover>();
        selectedOption = cm.GetSelectedOption();
    }
	
	void Update () {
        Vector2 direction = new Vector2(0,0);
        bool inputPressed = false;
        if (Input.GetButtonDown("Vertical") || Input.GetButtonDown("Horizontal"))
        {
            direction = direction + getDirection();
            enteringText = false;
            inputPressed = true;
      //      Debug.Log("DirectoinV: " + direction);

        }

        if (inputPressed) {
            cm.SetDirection(direction);
        }

        selectedOption = cm.GetSelectedOption();
        string clientState = "";
        MenuLink ml = selectedOption.GetComponent<MenuLink>();

        if (ml != null) {
            clientState = ml.GetState().ToString();
        }

        if (clientState == "inputfield")
        {
            inputField = ml.GetComponent<InputField>();
            
            enteringText = true;
            inputField.enabled = true;
            inputField.ActivateInputField();

        }
        else {
            enteringText = false;
            if (inputField != null) {
                inputField.enabled = false;
            }
            
        }

        if (!enteringText) {


            if (Input.GetButtonDown("Fire1"))
            {

                switch (clientState) {
                    
                    case "menu":
                        menuHandler.EnterMenu(ml.GetMenuItem(), activeMenu);
                        break;

                    case "register":
                        SubmitAccount("UsernameRegister", "PasswordRegister", ml, true);
                        break;

                    case "login":
                        SubmitAccount("UsernameLogin", "PasswordLogin", ml, false);
                        break;


                    default:
                        break;

                }
            }

            if (Input.GetButtonDown("Fire2"))
            {
                //rewrite this in future so that cancel only goes up a menu and does not enable previous menu
                //    enterMenu.SetActive(true);

                if (menuHandler.GetPrevious() == menuHandler.characterMenu && menuHandler.GetActiveMenu() == menuHandler.login)
                {
                    menuHandler.SetPrevious(menuHandler.home);
                }
                if (menuHandler.GetPrevious() != null)
                {
                    menuHandler.EnterMenu(menuHandler.GetPrevious(), activeMenu);
                    

                }

            }
        }
        direction = new Vector2(0, 0);
	}

    private Vector2 getDirection()
    { //if two buttons are pressed, it will only return the first occuring value rather than average them out
        Vector2 vecToReturn = new Vector2(0, 0);
        if (Input.GetKeyDown("up")) //up
        {
            vecToReturn = vecToReturn + Vector2.up;
        }
        if (Input.GetKeyDown("down")) // down
        {
            vecToReturn = vecToReturn + Vector2.down;
        }
        if (Input.GetKeyDown("right")) //right
        {
            vecToReturn = vecToReturn + Vector2.right;
        }
        if (Input.GetKeyDown("left")) //left
        {
            vecToReturn = vecToReturn + Vector2.left;
        }
        return vecToReturn;

    }

    private void SubmitAccount(string findUser, string findPass, MenuLink ml, bool registering)
    {
        
        GameObject passwordGameObj = GameObject.Find(findPass);
        InputField passwordInput = passwordGameObj.GetComponent<InputField>();

        GameObject userGameObj = GameObject.Find(findUser);
        InputField usernameInput = userGameObj.GetComponent<InputField>();

        GameObject subObj = ml.GetMenuItem();
        PacketProcessor packetProcessor = new PacketProcessor();
        string password = passwordInput.text;
        string userName = usernameInput.text;

        OpenStatusBox(); //need to change this so it uses a separate login status box with a menulink set to character select, script should read text and make sure it says login successful

        CheckInputs(userName, password);
        AccountPacket ap = new AccountPacket();
        byte[] data = ap.GetDataBytes(userName, password);

        
        SubPacket subPacket = new SubPacket(registering, (ushort)userName.Length, (ushort)password.Length, 0, 0, data, SubPacketTypes.Account);

        BasePacket packetToSend = BasePacket.CreatePacket(subPacket, false, false);

            packetProcessor.LoginOrRegister(packetToSend);



        

    }

    private void CheckInputs(string userName, string password)
    {
        if (password.Contains(" ") || userName.Contains(" "))
        {
            throw new Exception("Invalid character in Username or Password");
        }
        if (password == null && userName == null)
        {
            throw new Exception("Empty username or password");
        }
        if (password.Length < 4 || userName.Length < 3)
        {
            throw new Exception("Password and Username length must be greater than 4 characters");
        }
    }

    private void OpenStatusBox()
    {
        cursor = GameObject.Find("Cursor");
        menuHandler.SetCursor(cursor);
        menuHandler.ToggleCursor(false);
        menuHandler.OpenStatusBox();
    }
}
