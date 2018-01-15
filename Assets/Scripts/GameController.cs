using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    private GameObject player;

    private PlayerCameraSwitcher pcs;

    private bool debugMode = false;
    private bool menuMode = false;

    // UI Components
    public Image UI_Cursor;

    public GameObject UI_DebugPanel;
    public GameObject UI_DebugTextXYZ;
    private Text UI_DebugTextXYZ_text;
    public GameObject UI_DebugTextVersion;

    public GameObject UI_MenuPanel;
    public GameObject UI_MenuButtonReturnHome;

    private void Start() {
        
        Cursor.visible = false;
        
        player = GameObject.FindGameObjectWithTag("Player");

        pcs = player.GetComponent<PlayerCameraSwitcher>();

        UI_DebugTextXYZ_text = UI_DebugTextXYZ.GetComponent<Text>();

        setDebugMode(false);
        setMenuMode(false);

    }

    private void Update() {
        
        // Esc Key Handling (Menu Mode)
        if(Input.GetKeyDown(KeyCode.Escape)) {
            setMenuMode(!menuMode);
        }

        // F3 Key Handling (Debug Mode)
        if(Input.GetKeyDown(KeyCode.F3)) {
            setDebugMode(!debugMode);
        }

        // F5 Key Handling (Camera switch)
        if(Input.GetKeyDown(KeyCode.F5)) {
            pcs.SwitchCamera();
            UI_Cursor.enabled = (pcs.activeCamera == 0);
        }

        // Player Position Update
        if (debugMode) {
            Vector3 pos = player.transform.position;
            UI_DebugTextXYZ_text.text = " XYZ: " + pos.x.ToString("0.0000") + "/" + pos.y.ToString("0.0000") + "/" + pos.z.ToString("0.0000");
        }

    }

    private void setDebugMode(bool mode) {
        debugMode = mode;
        UI_DebugPanel.SetActive(debugMode);
        UI_DebugTextVersion.SetActive(debugMode);
        UI_DebugTextXYZ.SetActive(debugMode);
    }

    private void setMenuMode(bool mode) {
        
        menuMode = mode;
        UI_MenuPanel.SetActive(menuMode);
        UI_MenuButtonReturnHome.SetActive(menuMode);
        Cursor.visible = menuMode;

        // Disable Movement
        if (player != null) {
            PlayerMove pm = player.GetComponent<PlayerMove>();
            pm.head.GetComponent<CameraMovement>().enabled = !menuMode;
            pm.enabled = !menuMode;
            Pointer p = player.GetComponent<Pointer>();
            p.enabled = !menuMode;
        }
        var cameras = GameObject.FindGameObjectsWithTag("MainCamera");
        foreach(GameObject cam in cameras) {
            CameraMovement cm = cam.GetComponent<CameraMovement>();
            if (cm != null) cm.enabled = !menuMode;
        }

    }

}
