using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {

    public byte[,,] blocks;

    private Mesh mesh;
    private MeshFilter mf;
    private MeshRenderer mr;
    private MeshCollider mc;

    public Material tileset;

    public int chunkX, chunkY;

    private void Awake() {
        
        blocks = new byte[Constants.chunkSizeX, Constants.chunkSizeY, Constants.chunkSizeZ];

        if (mf == null) {
            mf = gameObject.AddComponent<MeshFilter>();
        }
        if (mr == null) {
            mr = gameObject.AddComponent<MeshRenderer>();
        }
        if (mc == null) {
            mc = gameObject.AddComponent<MeshCollider>();
        }

        // Transparency Settings
        mr.material = tileset;
        mr.material.SetOverrideTag("RenderType", "TransparentCutout");
        mr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mr.material.SetInt("_ZWrite", 1);
        mr.material.EnableKeyword("_ALPHATEST_ON");
        mr.material.DisableKeyword("_ALPHABLEND_ON");
        mr.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mr.material.SetFloat("_SpecularHighlights", 0f);
        mr.material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        mr.material.renderQueue = 2450;

    }

    public void Render() {

        mesh = GenerateMesh();

        mf.mesh = mesh;
        mc.sharedMesh = mesh;

    }

    public static string getId(int x, int y) {
        return x.ToString("X8") + y.ToString("X8");
    }

    public ChunkData makeChunkData() {

        ChunkData data = new ChunkData();

        data.id            = getId(chunkX, chunkY);
        data.chunkX        = this.chunkX;
        data.chunkY        = this.chunkY;
        data.blockData     = this.blocks;

        // data.verticesData  = ArrayCast.Cast(this.mesh.vertices);
        // data.trianglesData = this.mesh.triangles;
        // data.uvData        = ArrayCast.Cast(this.mesh.uv);

        return data;

    }

    private Mesh GenerateMesh() {

        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        float UVr = 1.0f / 16.0f;

        for(int x = 0; x < Constants.chunkSizeX; x++) {
            for(int y = 0; y < Constants.chunkSizeY; y++) {
                for(int z = 0; z < Constants.chunkSizeZ; z++) {
                    if( !Constants.isTransparent(blocks[x, y, z]) ) { // condition : block is solid
                    
                        int texture;
                        
                        // RIGHT
                        if( x == Constants.chunkSizeX - 1 || Constants.isTransparent(blocks[x + 1, y, z]) ) {
                            int vIndex = vertices.Count;
                            vertices.Add(new Vector3(x + 1, y    , z    )); // vIndex
                            vertices.Add(new Vector3(x + 1, y    , z + 1)); // vIndex + 1
                            vertices.Add(new Vector3(x + 1, y + 1, z    )); // vIndex + 2
                            vertices.Add(new Vector3(x + 1, y + 1, z + 1)); // vIndex + 3
                            int[] newTriangles = { 0, 2, 1, 1, 2, 3 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Constants.Resource[blocks[x, y, z], 1];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            Vector2 tileOffset = new Vector2((float)UVx, (float)UVy) * UVr;

                            uvs.Add( new Vector2( 0f,  0f) + tileOffset );
                            uvs.Add( new Vector2(UVr,  0f) + tileOffset );
                            uvs.Add( new Vector2( 0f, UVr) + tileOffset );
                            uvs.Add( new Vector2(UVr, UVr) + tileOffset );
                        }

                        // LEFT
                        if ( x == 0 || Constants.isTransparent(blocks[x - 1, y, z]) ) {
                            int vIndex = vertices.Count;
                            vertices.Add(new Vector3(x, y, z)); // vIndex
                            vertices.Add(new Vector3(x, y, z + 1)); // vIndex + 1
                            vertices.Add(new Vector3(x, y + 1, z)); // vIndex + 2
                            vertices.Add(new Vector3(x, y + 1, z + 1)); // vIndex + 3
                            int[] newTriangles = { 0, 1, 2, 1, 3, 2 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Constants.Resource[blocks[x, y, z], 2];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            Vector2 tileOffset = new Vector2((float)UVx, (float)UVy) * UVr;

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVr, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVr) + tileOffset);
                            uvs.Add(new Vector2(UVr, UVr) + tileOffset);
                        }

                        // TOP
                        if( y == Constants.chunkSizeY - 1 || Constants.isTransparent(blocks[x, y + 1, z]) ) {
                            int vIndex = vertices.Count;
                            vertices.Add(new Vector3(x, y + 1, z)); // vIndex
                            vertices.Add(new Vector3(x + 1, y + 1, z)); // vIndex + 1
                            vertices.Add(new Vector3(x, y + 1, z + 1)); // vIndex + 2
                            vertices.Add(new Vector3(x + 1, y + 1, z + 1)); // vIndex + 3
                            int[] newTriangles = { 0, 2, 1, 1, 2, 3 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Constants.Resource[blocks[x, y, z], 0];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            Vector2 tileOffset = new Vector2((float)UVx, (float)UVy) * UVr;

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVr, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVr) + tileOffset);
                            uvs.Add(new Vector2(UVr, UVr) + tileOffset);
                        }

                        // BOT
                        if( y == 0 || Constants.isTransparent(blocks[x, y - 1, z]) ) {
                            int vIndex = vertices.Count;
                            vertices.Add(new Vector3(x, y, z)); // vIndex
                            vertices.Add(new Vector3(x + 1, y, z)); // vIndex + 1
                            vertices.Add(new Vector3(x, y, z + 1)); // vIndex + 2
                            vertices.Add(new Vector3(x + 1, y, z + 1)); // vIndex + 3
                            int[] newTriangles = { 0, 1, 2, 1, 3, 2 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Constants.Resource[blocks[x, y, z], 5];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            Vector2 tileOffset = new Vector2((float)UVx, (float)UVy) * UVr;

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVr, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVr) + tileOffset);
                            uvs.Add(new Vector2(UVr, UVr) + tileOffset);
                        }

                        // BACK
                        if( z == Constants.chunkSizeZ - 1 || Constants.isTransparent(blocks[x, y, z + 1]) ) {
                            int vIndex = vertices.Count;
                            vertices.Add(new Vector3(x, y, z + 1)); // vIndex
                            vertices.Add(new Vector3(x + 1, y, z + 1)); // vIndex + 1
                            vertices.Add(new Vector3(x, y + 1, z + 1)); // vIndex + 2
                            vertices.Add(new Vector3(x + 1, y + 1, z + 1)); // vIndex + 3
                            int[] newTriangles = { 0, 1, 2, 1, 3, 2 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Constants.Resource[blocks[x, y, z], 4];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            Vector2 tileOffset = new Vector2((float)UVx, (float)UVy) * UVr;

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVr, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVr) + tileOffset);
                            uvs.Add(new Vector2(UVr, UVr) + tileOffset);
                        }

                        // FRONT
                        if( z == 0 || Constants.isTransparent(blocks[x, y, z - 1]) ) {
                            int vIndex = vertices.Count;
                            vertices.Add(new Vector3(x, y, z)); // vIndex
                            vertices.Add(new Vector3(x + 1, y, z)); // vIndex + 1
                            vertices.Add(new Vector3(x, y + 1, z)); // vIndex + 2
                            vertices.Add(new Vector3(x + 1, y + 1, z)); // vIndex + 3
                            int[] newTriangles = { 0, 2, 1, 1, 2, 3 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Constants.Resource[blocks[x, y, z], 3];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            Vector2 tileOffset = new Vector2((float)UVx, (float)UVy) * UVr;

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVr, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVr) + tileOffset);
                            uvs.Add(new Vector2(UVr, UVr) + tileOffset);
                        }

                    }
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        return mesh;

    }

}
