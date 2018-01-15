using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapNameInputHandler : MonoBehaviour {

    // References
    public Text alertText;
    public Button createMapButton;

    private void Start() {
        alertText.text = "";
        createMapButton.enabled = true;
    }

    public void OnEndEdit() {

        string str = gameObject.GetComponent<InputField>().text;

        if(str == "") {
            alertText.text = "Invalid name";
            createMapButton.enabled = false;
        } else if(Directory.Exists(Application.persistentDataPath + "/maps/" + str)) {
            alertText.text = "Map with given name already exists.";
            createMapButton.enabled = false;
        } else {
            alertText.text = "";
            createMapButton.enabled = true;
            createMapButton.GetComponent<MapSelectionButton>().path = Application.persistentDataPath + "/maps/" + str;
        }

    }

}
