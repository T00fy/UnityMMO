using UnityEngine;
using System;
using UnityEngine.UI;

public class CursorInput : MonoBehaviour {

    public GameObject selectedOption;
    public GameObject[] menuObjects;

    private GameObject cursor;
    private GameObject currentMenu ;
    private int counter;
    private float scaleOfCanvas;
	// Use this for initialization
	void Awake () {
        cursor = GameObject.Find("Cursor");
        string parent = cursor.transform.parent.name;
        currentMenu = GameObject.Find(parent);
        scaleOfCanvas = GameObject.Find("MainMenu").transform.localScale.x;
        counter = 0;

    }
	
	void Update () {

        if (Input.GetButtonDown("Vertical")) {
            DoVerticalInputs();
            
        }
        if (Input.GetButtonDown("Horizontal"))
        {
            DoHorizontalInputs();

        }

            if (Input.GetButtonDown("Fire1")) {
            MenuLink ml = selectedOption.GetComponent<MenuLink>();
            string type = ml.GetState().ToString();
            if (type == "menu") {
                GameObject enterMenu = ml.GetMenuItem();
                currentMenu.SetActive(false);
                enterMenu.SetActive(true);
            }
            if (type == "inputfield") {
                InputField inputField = ml.GetComponent<InputField>();
                inputField.Select();

            }

        }

	}

    private void DoHorizontalInputs()
    {
        var direction = getDirection();
        //-1 is right
        // 1 is left
        float current = menuObjects[counter].transform.position.y;
        float next = 0.0f;
        try
        {
            next = menuObjects[counter + direction].transform.position.y;

            if (!sameYPositions(next, current))
            { //hack to stop further execution of block
                throw new IndexOutOfRangeException();
            }
            counter = counter + direction;

            if (sameYPositions(next, current))
            {

                cursor.transform.position = new Vector3(GetCursorXPosition(menuObjects[counter]), menuObjects[counter].transform.position.y);
            }
        }
        catch (IndexOutOfRangeException)
        {
            //stay in current position, no valid input
            next = current;

            if (counter < 0)
            {
                counter = 0;
            }
            if (counter > menuObjects.Length - 1)
            {
                counter = menuObjects.Length;
            }
            return;
        }
        selectedOption = menuObjects[counter];
    }

    private void DoVerticalInputs()
    {
        var direction = getDirection();
        //-1 is up
        // 1 is down
        float current = menuObjects[counter].transform.position.y;
        float next = 0.0f;
        try
        {
            next = menuObjects[counter + direction].transform.position.y;

            if (sameYPositions(next, current))
            {
                counter = GetNextVerticalSelection(direction, current);
                cursor.transform.position = new Vector3(GetCursorXPosition(menuObjects[counter]), menuObjects[counter].transform.position.y);
            }
            else {
                counter = counter + direction;
                cursor.transform.position = new Vector3(GetCursorXPosition(menuObjects[counter]), menuObjects[counter].transform.position.y);
            }
        }
        catch (IndexOutOfRangeException)
        {
            //stay in current position, no valid input
            next = current;

            if (counter < 0)
            {
                counter = 0;
            }
            if (counter > menuObjects.Length - 1)
            {
                counter = menuObjects.Length - 1;
            }
            return;
        }
        selectedOption = menuObjects[counter];
    }

    private int GetNextVerticalSelection(int direction, float current)
    {
        for (int i = counter; i < menuObjects.Length; i = i + direction) {
                if (!sameYPositions(current, menuObjects[i].transform.position.y))
                {
                    return i;
                }
        }
        throw new IndexOutOfRangeException();
    }

    private bool sameYPositions(float pos1, float pos2)
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
            return -1;
        }
        if (Input.GetAxis("Vertical") < 0)
        {
            return 1;
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
