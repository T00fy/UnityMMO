using UnityEngine;
using System.Collections;

public class MenuLink : MonoBehaviour {
    public GameObject linkedMenuItem;
    public State state;
    // Use this for initialization

    public enum State
    {       //creates a drop down menu in unity editor of the available states
        menu, inputfield, register, login
    }

    public State GetState() {
        return state;
    }
    public GameObject GetMenuItem () {
        return linkedMenuItem;
	}
}
