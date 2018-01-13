using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour {

    public byte code;

    // Texture Mapping, 192 if custom generation
    public byte textureTop, textureBot, textureLeft, textureRight, textureFront, textureBack;
    public bool textureGenerated = false;

    public bool gravity = false;

    public float hardness;

    public bool selected = false;
    public float selectedCounter = 0.0f;

    public bool destroyFlag = false;
    public bool destroyConfirm = false;

    private void Update() {

        if (selected) {
            selectedCounter += Time.deltaTime;
            if (selectedCounter > hardness) destroyFlag = true;
        }

        if(destroyConfirm) {
            Destroy(gameObject);
        }

    }

    public void setSelected(bool s) {
        selected = s;
        if (!selected) selectedCounter = 0.0f;
    }


}
