using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatViewer : MonoBehaviour {
    public GameObject[] statNumbers;
    private GameObject decreaseArrow;
	// Use this for initialization
	void Start () {
        //get all arrows from statnumbers

	}
	
	// Update is called once per frame
	void Update () {
        foreach (var number in statNumbers)
        {
            decreaseArrow = number.transform.GetChild(0).gameObject;
            if (number.GetComponent<Text>().text == "1")
            {
                
                decreaseArrow.SetActive(false);
            }
            else
            {
                decreaseArrow.SetActive(true);
            }
        
        }
    }
}
