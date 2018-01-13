using System;
using UnityEngine;

public class BlockTextureMapper : MonoBehaviour {

    public enum Position { TOP = 0, FRONT = 1, BACK = 2, LEFT = 3, RIGHT = 4, BOTTOM = 5 };
    public Position position = Position.TOP;

    public Material tileset;

    private BlockController mBlockController;
    private GameController mGameController;

    private byte texture;

    private bool generated = false;

    private Mesh mesh;
    private MeshFilter mf;
    private MeshRenderer mr;

    private void Start() {

        mBlockController = GetComponentInParent<BlockController>();
        mGameController  = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        switch (position) {
            case Position.TOP:
                transform.Rotate(270.0f,   0.0f, 0.0f);
                break;
            case Position.BOTTOM:
                transform.Rotate( 90.0f,   0.0f, 0.0f);
                break;
            case Position.LEFT:
                transform.Rotate(  0.0f, 270.0f, 0.0f);
                break;
            case Position.RIGHT:
                transform.Rotate(  0.0f,  90.0f, 0.0f);
                break;
            case Position.FRONT:
                transform.Rotate(  0.0f, 180.0f, 0.0f);
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
        if(!generated) {
            try {
                Regenerate();
            } catch(ObjectNotCreatedException) {
                mGameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            }
        }
    }

    public void Regenerate() {

        switch (position) {
            case Position.TOP:
                texture = mBlockController.textureTop;
                break;
            case Position.BOTTOM:
                texture = mBlockController.textureBot;
                break;
            case Position.LEFT:
                texture = mBlockController.textureLeft;
                break;
            case Position.RIGHT:
                texture = mBlockController.textureRight;
                break;
            case Position.FRONT:
                texture = mBlockController.textureFront;
                break;
            case Position.BACK:
                texture = mBlockController.textureBack;
                break;
        }

        if(mf == null) {
            mf = gameObject.AddComponent<MeshFilter>();
        }
        if(mr == null) {
            mr = gameObject.AddComponent<MeshRenderer>();
            // Transparency Settings
            mr.material.SetOverrideTag("RenderType", "TransparentCutout");
            mr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mr.material.SetInt("_ZWrite", 1);
            mr.material.EnableKeyword("_ALPHATEST_ON");
            mr.material.DisableKeyword("_ALPHABLEND_ON");
            mr.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mr.material.renderQueue = 2450;
        }

        int x = 0, y = 0; float r = 1.0f;
        Vector2 tileOffset = Vector2.zero;

        if (texture == 0) {
            if (mGameController) mr.material.mainTexture = mGameController.TransparentTexture;
            else throw new ObjectNotCreatedException();
        } else if (texture == 192) {
            if (mGameController) mr.material.mainTexture = mGameController.GrassTopTexture;
            else throw new ObjectNotCreatedException();
        } else {

            texture--;
            mr.material = tileset;

            x = texture & 0xF;
            y = ~(texture >> 4);
            r = 1.0f / 16.0f;

            tileOffset = new Vector2((float)x, (float)y) * r;

        }

        mesh.uv = new Vector2[] { new Vector2(0f, 0f) + tileOffset,
                                      new Vector2( r, 0f) + tileOffset,
                                      new Vector2(0f,  r) + tileOffset,
                                      new Vector2( r,  r) + tileOffset };
        mf.mesh = mesh;

        generated = true;

    }

    private class ObjectNotCreatedException : Exception { }

}
