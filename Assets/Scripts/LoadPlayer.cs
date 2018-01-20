using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadPlayer : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        Character character = gameObject.AddComponent<Character>();
        character = Data.CHARACTER_ON_LOGIN;
    }
}

