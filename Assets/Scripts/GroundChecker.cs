using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour {

    private PlayerMove playerMove;

    private void Start() {
        playerMove = GetComponentInParent<PlayerMove>();
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Block") {
            playerMove.enableJump = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "Block") {
            playerMove.enableJump = false;
        }
    }

}
