using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    private GameObject player;
    private Rigidbody rb;

    private PlayerCameraSwitcher pcs;

    private bool debugMode = false;
    private bool menuMode = false;
    private bool onInventory = false;
    private bool cheatMode = false;

    // UI Components
    public Image UI_Cursor;

    public GameObject UI_DebugPanel;
    public GameObject UI_DebugTextXYZ;
    private Text UI_DebugTextXYZ_text;
    public GameObject UI_DebugTextVersion;

    public GameObject UI_MenuPanel;
    public GameObject UI_MenuButtonReturnHome;

    public Inventory inventoryManager;
    public GameObject UI_HotBarPanel;
    public GameObject UI_InventoryPanel;

    private void Start() {
        
        player = GameObject.FindGameObjectWithTag("Player");
        rb = player.GetComponent<Rigidbody>();

        pcs = player.GetComponent<PlayerCameraSwitcher>();

        UI_DebugTextXYZ_text = UI_DebugTextXYZ.GetComponent<Text>();

        setDebugMode(false);
        setMenuMode(false);
        setInventoryMode(false);
        cheatMode = false;

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

        // E Key Handling (Inventory open)
        if(Input.GetKeyDown(KeyCode.E)) {
            if(!menuMode) setInventoryMode(!onInventory);
        }


        // Cheat Mode
        if(Input.GetKeyDown(KeyCode.F12)) {
            cheatMode = !cheatMode;
            rb.useGravity = !cheatMode;
        }

        if(cheatMode) {
            player.transform.position = Vector3.Lerp(player.transform.position,
                                            new Vector3(player.transform.position.x, 85.0f, player.transform.position.z),
                                            3.0f * Time.deltaTime);
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

        if (onInventory) setInventoryMode(false);

        menuMode = mode;
        UI_MenuPanel.SetActive(menuMode);
        UI_MenuButtonReturnHome.SetActive(menuMode);
        UI_HotBarPanel.SetActive(!menuMode);
        setCursor(menuMode);
        disableMovement(!menuMode);

    }

    private void setInventoryMode(bool mode) {

        onInventory = mode;
        setCursor(onInventory);
        UI_MenuPanel.SetActive(onInventory);
        disableMovement(!onInventory);
        UI_InventoryPanel.SetActive(onInventory);

    }

    private void setCursor(bool mode) {

        Cursor.visible = mode;
        Cursor.lockState = mode ? CursorLockMode.None : CursorLockMode.Locked;

    }

    private void disableMovement(bool mode) {
    
        var cameras = player.GetComponentsInChildren<CameraMovement>();
        foreach (CameraMovement cam in cameras) {
            cam.enabled = mode;
        }

        if (player != null) {
            PlayerMove pm = player.GetComponent<PlayerMove>();
            pm.enabled = mode;
            Pointer p = player.GetComponent<Pointer>();
            p.enabled = mode;
        }

    }


}
