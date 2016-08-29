using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class TextUIListener : MonoBehaviour {
    MapGenerator mapGen;
    InputField inputField;
	// Use this for initialization
	void Start () {
        mapGen = FindObjectOfType<MapGenerator>();
        inputField = gameObject.GetComponent<InputField>();
        inputField.onEndEdit.AddListener(SendText);
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    private void SendText(string arg) {
        //Debug.Log("Arg: " + arg + "\n");
        mapGen.ChangeMap(arg);
    }
}
