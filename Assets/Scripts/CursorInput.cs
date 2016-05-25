using UnityEngine;
using System;
using UnityEngine.UI;

public class CursorInput : MonoBehaviour {

    public GameObject selectedOption;
    public GameObject[] menuObjects;
    public MenuHandler previousMenu;

    private GameObject cursor;
    private GameObject activeMenu;
    private int counter;
    private float scaleOfCanvas;
    private bool enteringText;
    private InputField inputField;
	// Use this for initialization
	void Awake () {
        cursor = gameObject;
        string parent = cursor.transform.parent.name;
        activeMenu = GameObject.Find(parent);
        scaleOfCanvas = GameObject.Find("MainMenu").transform.localScale.x;
        counter = 0;
    }
	
	void Update () {
              
        if (Input.GetButtonDown("Vertical"))
        {
            var direction = getDirection();
            enteringText = false;
            counter = GetClosestVerticalSelection(direction);

            cursor.transform.position = new Vector2(GetCursorXPosition(menuObjects[counter]), menuObjects[counter].transform.position.y);
            selectedOption = menuObjects[counter];

        }

        if (Input.GetButtonDown("Horizontal"))
        {
            var direction = getDirection();
            enteringText = false;

            counter = GetClosestHorizontalSelection(direction);
            cursor.transform.position = new Vector2(GetCursorXPosition(menuObjects[counter]), menuObjects[counter].transform.position.y);
            selectedOption = menuObjects[counter];
        }

        string type = "";
        MenuLink ml = selectedOption.GetComponent<MenuLink>();

        if (ml != null) {
            type = ml.GetState().ToString();
        }

        if (type == "inputfield")
        {
            inputField = ml.GetComponent<InputField>();
            
            enteringText = true;
            inputField.enabled = true;
            inputField.ActivateInputField();

        }
        else {
            if (inputField != null) {
                inputField.enabled = false;
            }
            
        }

        if (!enteringText) {


            if (Input.GetButtonDown("Fire1"))
            {

                switch (type) {

                    case "menu":
                        previousMenu.SetPrevious(activeMenu);
                        GameObject enterMenu = ml.GetMenuItem();
                        activeMenu.SetActive(false);
                        enterMenu.SetActive(true);
                        break;

                    case "register":
                        LoginOrRegister("UsernameRegister", "PasswordRegister", "register", ml);
                        break;

                    case "login":
                        LoginOrRegister("UsernameLogin", "PasswordLogin", "register", ml);
                        break;

                    default:
                        break;

                }
            }

            if (Input.GetButtonDown("Fire2"))
            {

                //    enterMenu.SetActive(true);
                if (previousMenu.GetPrevious() != null)
                {
                    activeMenu.SetActive(false);
                    previousMenu.GetPrevious().SetActive(true);

                }

            }
        }


	}

    private void LoginOrRegister(string findUser, string findPass, string cmdString, MenuLink ml)
    {
        GameObject passwordGameObj = GameObject.Find(findPass);
        InputField passwordInput = passwordGameObj.GetComponent<InputField>();

        GameObject userGameObj = GameObject.Find(findUser);
        InputField usernameInput = userGameObj.GetComponent<InputField>();

        GameObject submit = ml.GetMenuItem();
        var menuEnter = submit.GetComponent<ClickRegister>();
        string password = passwordInput.text;
        string userName = usernameInput.text;
        string cmd = cmdString + " " + userName + " " + password;

        menuEnter.StartConnection(cmd, userName, password);
    }

    private int GetClosestVerticalSelection(Vector2 direction)
    {
        var curr = menuObjects[counter].transform.position;
        curr.x = curr.x - 2.0f;
        float smallestDistance = 9999999999999f;
        
        int pointer = -1;
        for (int i = 0; i < menuObjects.Length; i++) {
            if (!samePositions(curr.y, menuObjects[i].transform.position.y)) {
                Vector2 midVector = menuObjects[i].transform.position - curr;
                Vector2 directionOfMid = new Vector2(0,0);
                if (midVector.normalized.y < 0)
                {
                    directionOfMid = new Vector2(0,-1);
                }
                else
                {
                    directionOfMid = new Vector2(0,1);
                }
                if (Vector2.Distance(curr, menuObjects[i].transform.position) < smallestDistance && directionOfMid.normalized.y == direction.normalized.y && !menuObjects[counter].Equals(menuObjects[i])) {
                    //and if the vector between a and b normalized in y direction is same as direction
                    smallestDistance = Vector2.Distance(curr, menuObjects[i].transform.position);
                    pointer = i;
                }
            }
            
        }
        if (pointer == -1) {
            return counter;
        }
        return pointer;
    }


    private int GetClosestHorizontalSelection(Vector2 direction)
    {
        var curr = menuObjects[counter].transform.position;
        curr.x = curr.x - 2.0f;
        float smallestDistance = 9999999999999f;
        Debug.Log(direction);
        int pointer = -1;
        for (int i = 0; i < menuObjects.Length; i++)
        {
            if (samePositions(curr.y, menuObjects[i].transform.position.y))
            {
                
                //to fix: pushing down twice at length of array will cause the cursor to go up
                Vector2 midVector = menuObjects[i].transform.position - curr;
                Debug.Log("horizontal mid: " + midVector);
                Debug.Log("horizontal dir: " + direction);
          /*      Vector2 directionOfMid = new Vector2(0,0);
                if (midVector.normalized.x < 0)
                {
                    directionOfMid = new Vector2(-1, 0);
                }
                else
                {
                    directionOfMid = new Vector2(1, 0);
                }*/
                if (Vector2.Distance(curr, menuObjects[i].transform.position) < smallestDistance && midVector.normalized == direction.normalized && !menuObjects[counter].Equals(menuObjects[i]))
                {
                    //and if the vector between a and b normalized in y direction is same as direction
                    smallestDistance = Vector2.Distance(curr, menuObjects[i].transform.position);
                    pointer = i;
                    
                }
            }

        }
        if (pointer == -1)
        {
            return counter;
        }
        return pointer;
    }


    private bool samePositions(float pos1, float pos2)
    {
        if (Mathf.Approximately(pos1, pos2)) {
            return true;
        }
        return false;
    }

    private float GetCursorXPosition(GameObject selected)
    {
        RectTransform rt = selected.GetComponent<RectTransform>();
        float x = selected.transform.position.x/scaleOfCanvas;
        float cursorPos = x -( rt.rect.width / 2.0f);
        cursorPos = cursorPos - 40.0f;

        //have to multiply by the scale set in canvas
        cursorPos = cursorPos * scaleOfCanvas;

        
        return cursorPos;
    }

    private Vector2 getDirection()
    {
        //problem is if a diagonal input is entered, axis will average both results of a vertical/horizontal axis call
        if (Input.GetAxis("Vertical") > 0) //up
        {
            return Vector2.up;
        }
        if (Input.GetAxis("Vertical") < 0) // down
        {
            return Vector2.down;
        }
        if (Input.GetAxis("Horizontal") > 0) //right
        {
            return Vector2.right;
        }
        if (Input.GetAxis("Horizontal") < 0) //left
        {
            return Vector2.left;
        }
        return new Vector2(0,0);
    }
}
