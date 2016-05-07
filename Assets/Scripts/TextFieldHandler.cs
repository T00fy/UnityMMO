using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextFieldHandler : MonoBehaviour {
    private string input;
	// Use this for initialization
	void Start () {
        var input = gameObject.GetComponent<InputField>();

        input.onEndEdit.AddListener(SetInput);
    }
    private void SetInput(string inputFieldString) {
        input = inputFieldString;
        Debug.Log(input);
    }

    public string GetInput() {
        return input;
    }

}
