using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour {

    public GameObject mPointer;
    private BlockManager bm;

    private void Start() {
        bm = GameObject.FindGameObjectWithTag("GameController").GetComponent<BlockManager>();
    }

    private void Update() {
        
        if(Input.GetMouseButton(0)) { // Mouse Left Click On
            RaycastHit hit;
            if (Physics.Raycast(mPointer.transform.position, mPointer.transform.forward, out hit, 4.5f)) {
                if (hit.collider.gameObject != null) {
                    if (hit.collider.gameObject.tag == "Block") {
                        bm.setSelected(true, hit.point, -mPointer.transform.forward, mPointer.transform.up);
                    }
                } else {
                    // no object selected
                    bm.setSelected(false);
                }
            }
        } else if(Input.GetMouseButtonUp(0)) { // Mouse Left Click Release
            bm.setSelected(false);
        } else if(Input.GetMouseButtonDown(1)) { // Mouse Right Click On
            RaycastHit hit;
            if(Physics.Raycast(mPointer.transform.position, mPointer.transform.forward, out hit, 4.5f)) {
                if(hit.collider.gameObject != null) {
                    if(hit.collider.gameObject.tag == "Block") {
                        bm.PlaceBlock(hit.point, transform.position);
                    }
                }
            }
        }
        

    }

}
