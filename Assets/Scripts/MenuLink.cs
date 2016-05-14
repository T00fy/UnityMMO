using UnityEngine;
using System.Collections;

public class MenuLink : MonoBehaviour {
    public GameObject linkedMenuItem;
    public State current;
    // Use this for initialization

    public enum State
    {       //creates a drop down menu in unity editor of the available states
        menu, inputfield
    }

    public State GetState() {
        return current;
    }
    public GameObject GetMenuItem () {
        return linkedMenuItem;
	}
}
