using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrackMapper : MonoBehaviour {

    public enum Position { TOP = 0, FRONT = 1, BACK = 2, LEFT = 3, RIGHT = 4, BOTTOM = 5 };
    public Position position = Position.TOP;

    public Material tileset;

    private BlockController mBlockController;

    private int phase = 0;

    private Mesh mesh;
    private MeshFilter mf;
    private MeshRenderer mr;

    private void Start() {

        mBlockController = GetComponentInParent<BlockController>();
        
        switch (position) {
            case Position.TOP:
                transform.Rotate(270.0f, 0.0f, 0.0f);
                break;
            case Position.BOTTOM:
                transform.Rotate(90.0f, 0.0f, 0.0f);
                break;
            case Position.LEFT:
                transform.Rotate(0.0f, 270.0f, 0.0f);
                break;
            case Position.RIGHT:
                transform.Rotate(0.0f, 90.0f, 0.0f);
                break;
            case Position.FRONT:
                transform.Rotate(0.0f, 180.0f, 0.0f);
                break;
            case Position.BACK:

                break;
        }

        mesh = new Mesh();
        mesh.vertices = new Vector3[] { new Vector3(-0.5f, -0.5f, 0f),
                                            new Vector3(+0.5f, -0.5f, 0f),
                                            new Vector3(-0.5f, +0.5f, 0f),
                                            new Vector3(+0.5f, +0.5f, 0f) };
        mesh.normals = new Vector3[] { new Vector3(0f, 0f, 1f),
                                            new Vector3(0f, 0f, 1f),
                                            new Vector3(0f, 0f, 1f),
                                            new Vector3(0f, 0f, 1f) };
        mesh.triangles = new int[] { 0, 1, 2, 1, 3, 2 };

    }

    private void Update() {
        if (mBlockController.selected) {
            int curPhase = Mathf.Min(10, 1 + Mathf.FloorToInt(mBlockController.selectedCounter * 10.0f / mBlockController.hardness));
            if(curPhase != phase) {
                phase = curPhase;
                Regenerate();
            }
        } else {
            phase = 0;
            Regenerate();
        }
    }

    public void Regenerate() {

        int texture = 239 + phase;

        if (mf == null) {
            mf = gameObject.AddComponent<MeshFilter>();
        }
        if (mr == null) {
            mr = gameObject.AddComponent<MeshRenderer>();
            // Transparency Settings
            mr.material = tileset;
            mr.material.SetOverrideTag("RenderType", "TransparentCutout");
            mr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mr.material.SetInt("_ZWrite", 1);
            mr.material.EnableKeyword("_ALPHATEST_ON");
            mr.material.DisableKeyword("_ALPHABLEND_ON");
            mr.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mr.material.renderQueue = 2450;
        }

        if (phase == 0) {
            if(mf != null) {
                Destroy(mf);
            }
            if(mr != null) {
                Destroy(mr);
            }
        }

        int x = 0, y = 0; float r = 1.0f;
        Vector2 tileOffset = Vector2.zero;

        x = texture & 0xF;
        y = ~(texture >> 4);
        r = 1.0f / 16.0f;

        tileOffset = new Vector2((float)x, (float)y) * r;

        mesh.uv = new Vector2[] { new Vector2(0f, 0f) + tileOffset,
                                      new Vector2( r, 0f) + tileOffset,
                                      new Vector2(0f,  r) + tileOffset,
                                      new Vector2( r,  r) + tileOffset };
        mf.mesh = mesh;

    }

}
