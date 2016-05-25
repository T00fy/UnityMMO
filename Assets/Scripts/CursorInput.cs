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
            enteringText = false;
            DoVerticalInputs();

        }
        if (Input.GetButtonDown("Horizontal"))
        {
            enteringText = false;
            DoHorizontalInputs();
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


                if (type == "menu")
                {
                    
                    previousMenu.SetPrevious(activeMenu);
                    GameObject enterMenu = ml.GetMenuItem();

                    activeMenu.SetActive(false);
                    enterMenu.SetActive(true);
                }

                if (type == "register")
                {
                    GameObject passwordGameObj = GameObject.Find("PasswordRegister");
                    InputField passwordInput = passwordGameObj.GetComponent<InputField>();

                    GameObject userGameObj = GameObject.Find("UsernameRegister");
                    InputField usernameInput = userGameObj.GetComponent<InputField>();

                    GameObject submit = ml.GetMenuItem();
                    var register = submit.GetComponent<ClickRegister>();
                    string password = passwordInput.text;
                    string userName = usernameInput.text;
                    string cmd = "register " + userName + " " + password;

                    register.StartConnection(cmd, userName, password);

                }

                if (type == "login")
                {
                    GameObject passwordGameObj = GameObject.Find("PasswordLogin");
                    InputField passwordInput = passwordGameObj.GetComponent<InputField>();

                    GameObject userGameObj = GameObject.Find("UsernameLogin");
                    InputField usernameInput = userGameObj.GetComponent<InputField>();

                    GameObject submit = ml.GetMenuItem();
                    var login = submit.GetComponent<ClickRegister>();
                    string password = passwordInput.text;
                    string userName = usernameInput.text;
                    string cmd = "login " + userName + " " + password;

                    login.StartConnection(cmd, userName, password);

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

    private void DoHorizontalInputs()
    {
        var direction = getDirection();
        //-1 is right
        // 1 is left
        float current = menuObjects[counter].transform.position.y;

        counter = GetClosestHorizontalSelection(direction);
        cursor.transform.position = new Vector3(GetCursorXPosition(menuObjects[counter]), menuObjects[counter].transform.position.y);
        selectedOption = menuObjects[counter];
    }

    private void DoVerticalInputs()
    {
        var direction = getDirection();
        //-1 is up
        // 1 is down
        float current = menuObjects[counter].transform.position.y;

            counter = GetClosestVerticalSelection(direction);
            cursor.transform.position = new Vector3(GetCursorXPosition(menuObjects[counter]), menuObjects[counter].transform.position.y);
        selectedOption = menuObjects[counter];
    }




    private int GetClosestVerticalSelection(int direction)
    {
        var curr = menuObjects[counter].transform.position;
        curr.x = curr.x - 2.0f;
        float smallestDistance = 9999999999999f;
        
        int pointer = -1;
        for (int i = 0; i < menuObjects.Length; i++) {
            if (!samePositions(curr.y, menuObjects[i].transform.position.y)) {

                var midVector = menuObjects[i].transform.position - curr;
                var directionOfMid = 0;
                if (midVector.normalized.y < 0)
                {
                    directionOfMid = -1;
                }
                else {
                    directionOfMid = 1;
                }
                if (Vector2.Distance(curr, menuObjects[i].transform.position) <= smallestDistance && directionOfMid == direction) {
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

    private int GetClosestHorizontalSelection(int direction)
    {
        var curr = menuObjects[counter].transform.position;
        curr.x = curr.x - 2.0f;
        float smallestDistance = 9999999999999f;

        int pointer = -1;
        for (int i = 0; i < menuObjects.Length; i++)
        {
            if (samePositions(curr.y, menuObjects[i].transform.position.y))
            {
                
                //to fix: pushing down twice at length of array will cause the cursor to go up
                var midVector = menuObjects[i].transform.position - curr;
                var directionOfMid = 0;
                if (midVector.normalized.x < 0)
                {
                    directionOfMid = -1;
                }
                else {
                    directionOfMid = 1;
                }
                if (Vector2.Distance(curr, menuObjects[i].transform.position) <= smallestDistance && directionOfMid == direction && !menuObjects[counter].Equals(menuObjects[i]))
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

    private int getDirection()
    {
        if (Input.GetAxis("Vertical") > 0)
        {
            return 1;
        }
        if (Input.GetAxis("Vertical") < 0)
        {
            return -1;
        }
        if (Input.GetAxis("Horizontal") > 0)
        {
            return 1;
        }
        if (Input.GetAxis("Horizontal") < 0)
        {
            return -1;
        }
        return 1;
    }
}
