using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPointer : MonoBehaviour {

    public GameObject mPointer;

    private GameObject hitObject;
    private Chunk hitChunk = null;

    private void Update() {
        
        if(Input.GetMouseButton(0)) { // Mouse Left Click On
            RaycastHit hit;
            if (Physics.Raycast(mPointer.transform.position, mPointer.transform.forward, out hit, 4.5f)) {
                if (hit.collider.gameObject != null) {
                    if (hit.collider.gameObject.tag == "Block") {

                        // collided on same chunk
                        if (hitChunk != null && hitObject == hit.collider.gameObject) {

                            hitChunk.setSelected(true, hit.point);

                        } else { // collided on different chunk
                            if (hitChunk != null) {
                                hitChunk.setSelected(false, Vector3.zero);
                            }
                            hitObject = hit.collider.gameObject;
                            hitChunk  = hitObject.GetComponent<Chunk>();

                            hitChunk.setSelected(true, hit.point);

                        }

                    }
                } else {
                    // no object selected
                    hitChunk.setSelected(false, Vector3.zero);
                    hitChunk = null;
                }
            }
        } else if(Input.GetMouseButtonUp(0)) { // Mouse Left Click Release
            if (hitChunk != null) {
                hitChunk.setSelected(false, Vector3.zero);
                hitChunk = null;
            }
        } else if(Input.GetMouseButtonDown(1)) { // Mouse Right Click On
            RaycastHit hit;
            if(Physics.Raycast(mPointer.transform.position, mPointer.transform.forward, out hit, 4.5f)) {
                if(hit.collider.gameObject != null) {
                    if(hit.collider.gameObject.tag == "Block") {
                        hitChunk = hit.collider.gameObject.GetComponent<Chunk>();
                        if(hitChunk != null) {
                            hitChunk.PlaceBlock(hit.point);
                        }
                    }
                }
            }
        }
        

    }

}
