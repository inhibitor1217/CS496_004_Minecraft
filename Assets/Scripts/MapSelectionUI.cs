using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectionUI : MonoBehaviour {

    public GameObject textPrefab;
    public GameObject loadButtonPrefab;
    public GameObject deleteButtonPrefab;

    public GameObject startNewMapButton;

    private RectTransform rt;

    private List<GameObject> uiObjects;

    private void Start() {

        startNewMapButton.GetComponent<MapSelectionButton>().path = Application.persistentDataPath + "/maps/New World";

        uiObjects = new List<GameObject>();

        CreateUI();

    }

    private void CreateUI() {

        try {

            string[] maps = Directory.GetDirectories(Application.persistentDataPath + "/maps");

            for (int i = 0; i < maps.Length; i++) {

                // Text Object displaying map name
                GameObject textObj = Instantiate(textPrefab);
                uiObjects.Add(textObj);

                textObj.transform.SetParent(transform);

                rt = textObj.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(30.0f, -15.0f - 60.0f * i);
                rt.localScale = new Vector2(1.0f / 0.7f, 1.0f / 0.6f);

                Text t = textObj.GetComponent<Text>();
                t.text = maps[i].Replace(Application.persistentDataPath + "/maps\\", "");


                // Load Map Button
                GameObject loadButtonObj = Instantiate(loadButtonPrefab);
                uiObjects.Add(loadButtonObj);

                loadButtonObj.transform.SetParent(transform);

                rt = loadButtonObj.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(-270.0f, -15.0f - 60.0f * i);
                rt.localScale = new Vector2(1.0f / 0.7f, 1.0f / 0.6f);

                MapSelectionButton btnLoad = loadButtonObj.GetComponent<MapSelectionButton>();
                btnLoad.path = maps[i];


                // Delete Map Button
                GameObject deleteButtonObj = Instantiate(deleteButtonPrefab);
                uiObjects.Add(deleteButtonObj);

                deleteButtonObj.transform.SetParent(transform);

                rt = deleteButtonObj.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(-30.0f, -15.0f - 60.0f * i);
                rt.localScale = new Vector2(1.0f / 0.7f, 1.0f / 0.6f);

                MapSelectionButton btnDelete = deleteButtonObj.GetComponent<MapSelectionButton>();
                btnDelete.path = maps[i];

            }

        } catch (IOException) {
            print("Error : cannot find " + Application.persistentDataPath + "/maps directory");
        }

    }

    public void Reload() {
        
        foreach(GameObject obj in uiObjects) {
            Destroy(obj);
        }

        CreateUI();

    }

}
