using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatViewer : MonoBehaviour {
    public GameObject[] statNumbers;
    private GameObject decreaseArrow;
    private GameObject increaseArrow;
	// Use this for initialization
	void Start () {
        //get all arrows from statnumbers

	}
	
	// Update is called once per frame
	void Update () {
        foreach (var number in statNumbers)
        {
            decreaseArrow = number.transform.GetChild(0).gameObject;
            increaseArrow = number.transform.GetChild(1).gameObject;
            if (number.GetComponent<Text>().text == "1")
            {
                
                decreaseArrow.SetActive(false);
            }
            else
            {
                decreaseArrow.SetActive(true);
            }

            if (number.GetComponent<Text>().text == "9")
            {
                increaseArrow.SetActive(false);

            }
            else
            {
                increaseArrow.SetActive(true);
            }
        
        }
    }
}
