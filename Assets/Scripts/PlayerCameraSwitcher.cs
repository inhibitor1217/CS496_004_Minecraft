using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraSwitcher : MonoBehaviour {

    public Camera[] cameras = new Camera[3];

    public int activeCamera = 0;
    public SkinnedMeshRenderer visible;

    private void Start() {
        for (int i = 0; i < 3; i++) {
            cameras[i].enabled = (i == activeCamera);
        }
        if (activeCamera == 0) visible.enabled = false;
    }

    public void SwitchCamera() {
        activeCamera = (activeCamera + 1) % 3;
        for (int i = 0; i < 3; i++) {
            cameras[i].enabled = (i == activeCamera);
        }
        visible.enabled = (activeCamera != 0);
    }

}
