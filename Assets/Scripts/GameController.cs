using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    private GameObject player;

    private PlayerCameraSwitcher pcs;

    // UI Components
    public Image UI_Cursor;

    // Block Prefabs;
    public GameObject[] blockPrefabs = new GameObject[128];

    // Generated Textures
    private float[,,] textureRandomArray;

    public Texture2D GrassTopTexture;
    public Texture2D TransparentTexture;

    private void Start() {
        
        Cursor.visible = false;
        
        player = GameObject.FindGameObjectWithTag("Player");

        pcs = player.GetComponent<PlayerCameraSwitcher>();

        // Generate Blocks
        
        // Generate Textures
        /*
        textureRandomArray = GenerateTextureArray();

        GrassTopTexture = GenerateTextureFromColor(new Color(0.25f, 0.7f, 0.25f));
        TransparentTexture = GenerateTransparentTexture();
        */
    }

    private void Update() {
        
        // F5 Key Handling (Camera switch)
        if(Input.GetKeyDown(KeyCode.F5)) {
            pcs.SwitchCamera();
            UI_Cursor.enabled = (pcs.activeCamera == 0);
        }

    }

    /*
    private float[,,] GenerateTextureArray() {

        float[,,] arr = new float[16, 16, 3];

        for (int i = 0; i < 16; i++) {
            for (int j = 0; j < 16; j++) {
                for(int k = 0; k < 3; k++) {
                    arr[i, j, k] = Random.Range(-0.08f, 0.08f);
                }
            }
        }

        return arr;

    }

    private Texture2D GenerateTextureFromColor(Color c) {

        Texture2D texture = new Texture2D(16, 16, TextureFormat.ARGB32, false);

        for(int i = 0; i < texture.width; i++) {
            for(int j = 0; j < texture.height; j++) {
                texture.SetPixel(i, j, new Color(c.r + textureRandomArray[i, j, 0],
                                                 c.g + textureRandomArray[i, j, 1],
                                                 c.b + textureRandomArray[i, j, 2]));
            }
        }
        texture.Apply();
        texture.filterMode = FilterMode.Point;

        return texture;

    }

    private Texture2D GenerateTransparentTexture() {

        Texture2D texture = new Texture2D(16, 16, TextureFormat.ARGB32, false);

        for (int i = 0; i < texture.width; i++) {
            for (int j = 0; j < texture.height; j++) {
                texture.SetPixel(i, j, new Color(1.0f, 1.0f, 1.0f, 0.0f));
            }
        }
        texture.Apply();
        texture.filterMode = FilterMode.Point;

        return texture;

    }

    */

}
