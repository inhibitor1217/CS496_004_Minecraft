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

    public static string getId(int x, int y) {
        return x.ToString("X8") + y.ToString("X8");
    }

    public ChunkData makeChunkData() {
        
        ChunkData data = new ChunkData();

        data.id            = getId(chunkX, chunkY);
        data.chunkX        = this.chunkX;
        data.chunkY        = this.chunkY;
        data.blockData     = encodeByteArray(this.blocks);

        // data.verticesData  = ArrayCast.Cast(this.mesh.vertices);
        // data.trianglesData = this.mesh.triangles;
        // data.uvData        = ArrayCast.Cast(this.mesh.uv);

        return data;

    }

    public static byte[] encodeByteArray(byte[,,] data) {

        List<byte> ret = new List<byte>();

        for(int x = 0; x < data.GetLength(0); x++) {
            for(int z = 0; z < data.GetLength(2); z++) {
                byte curblock = 0xFF, count = 0x00;
                for(int y = 0; y < data.GetLength(1); y++) {
                    if(data[x, y, z] != curblock) {
                        if(count != 0x00) {
                            ret.Add(curblock);
                            ret.Add(count);
                        }
                        curblock = data[x, y, z];
                        count = 0x01;
                    } else {
                        count++;
                    }
                }
                ret.Add(curblock);
                ret.Add(count);
                ret.Add(0xFF);
                ret.Add(0xFF);
            }
        }

        return ret.ToArray();

    }

    public static byte[,,] decodeByteArray(byte[] compress) {

        byte[,,] ret = new byte[Constants.chunkSizeX, Constants.chunkSizeY, Constants.chunkSizeZ];

        int x = 0, y = 0, z = 0;
        for (int i = 0; i < compress.Length; i += 2) {
            if (compress[i] == 0xFF) {
                y = 0;
                z++;
                if (z >= ret.GetLength(2)) {
                    z = 0; x++;
                }
                continue;
            }
            byte curblock = compress[i];
            byte curlength = compress[i + 1];
            while (curlength != 0x00) {
                ret[x, y, z] = curblock;
                curlength--; y++;
            }
        }

        return ret;

    }

    public void Render() {

        mf.mesh = GenerateMesh();
        mc.sharedMesh = GenerateSolidMesh();

    }

    private Mesh GenerateMesh() {

        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        float UVrX = 1.0f / 16.0f, UVrY = 1.0f / 32.0f;

        for(int x = 0; x < Constants.chunkSizeX; x++) {
            for(int y = 0; y < Constants.chunkSizeY; y++) {
                for(int z = 0; z < Constants.chunkSizeZ; z++) {
                    if( blocks[x, y, z] != 0x00 ) {

                        int texture;

                        if (Constants.isDiagonal(blocks[x, y, z])) {

                            texture = Constants.Resource[blocks[x, y, z], 0];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            Vector2 tileOffset = new Vector2((float)UVx * UVrX, (float)UVy * UVrY);

                            int vIndex = vertices.Count;
                            vertices.Add(new Vector3(x + 0.2f, y, z + 0.2f));
                            vertices.Add(new Vector3(x + 0.8f, y, z + 0.8f));
                            vertices.Add(new Vector3(x + 0.2f, y + 1, z + 0.2f));
                            vertices.Add(new Vector3(x + 0.8f, y + 1, z + 0.8f));
                            
                            int[] newTriangles1 = { 0, 2, 1, 1, 2, 3 };
                            for (int i = 0; i < 6; i++) newTriangles1[i] += vIndex;
                            triangles.AddRange(newTriangles1);

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVrX, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVrY) + tileOffset);
                            uvs.Add(new Vector2(UVrX, UVrY) + tileOffset);

                            // -----------------------------------------------------

                            vIndex = vertices.Count;
                            vertices.Add(new Vector3(x + 0.2f, y, z + 0.2f));
                            vertices.Add(new Vector3(x + 0.8f, y, z + 0.8f));
                            vertices.Add(new Vector3(x + 0.2f, y + 1, z + 0.2f));
                            vertices.Add(new Vector3(x + 0.8f, y + 1, z + 0.8f));

                            int[] newTriangles2 = { 0, 1, 2, 1, 3, 2 };
                            for (int i = 0; i < 6; i++) newTriangles2[i] += vIndex;
                            triangles.AddRange(newTriangles2);

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVrX, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVrY) + tileOffset);
                            uvs.Add(new Vector2(UVrX, UVrY) + tileOffset);

                            // ------------------------------------------------------

                            vIndex = vertices.Count;
                            vertices.Add(new Vector3(x + 0.2f, y, z + 0.8f));
                            vertices.Add(new Vector3(x + 0.8f, y, z + 0.2f));
                            vertices.Add(new Vector3(x + 0.2f, y + 1, z + 0.8f));
                            vertices.Add(new Vector3(x + 0.8f, y + 1, z + 0.2f));

                            int[] newTriangles3 = { 0, 2, 1, 1, 2, 3 };
                            for (int i = 0; i < 6; i++) newTriangles3[i] += vIndex;
                            triangles.AddRange(newTriangles3);

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVrX, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVrY) + tileOffset);
                            uvs.Add(new Vector2(UVrX, UVrY) + tileOffset);

                            // ------------------------------------------------------

                            vIndex = vertices.Count;
                            vertices.Add(new Vector3(x + 0.2f, y, z + 0.8f));
                            vertices.Add(new Vector3(x + 0.8f, y, z + 0.2f));
                            vertices.Add(new Vector3(x + 0.2f, y + 1, z + 0.8f));
                            vertices.Add(new Vector3(x + 0.8f, y + 1, z + 0.2f));

                            int[] newTriangles4 = { 0, 1, 2, 1, 3, 2 };
                            for (int i = 0; i < 6; i++) newTriangles4[i] += vIndex;
                            triangles.AddRange(newTriangles4);

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVrX, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVrY) + tileOffset);
                            uvs.Add(new Vector2(UVrX, UVrY) + tileOffset);

                            continue;

                        }

                        bool isSmall = Constants.isSmall(blocks[x, y, z]);
                        float df = 1.0f / 16.0f;

                        // RIGHT
                        if (x == Constants.chunkSizeX - 1 || Constants.isTransparent(blocks[x + 1, y, z])) {
                            int vIndex = vertices.Count;
                            if (!isSmall) {
                                vertices.Add(new Vector3(x + 1, y, z)); // vIndex
                                vertices.Add(new Vector3(x + 1, y, z + 1)); // vIndex + 1
                                vertices.Add(new Vector3(x + 1, y + 1, z)); // vIndex + 2
                                vertices.Add(new Vector3(x + 1, y + 1, z + 1)); // vIndex + 3
                            } else {
                                vertices.Add(new Vector3(x + 1 - df, y, z)); // vIndex
                                vertices.Add(new Vector3(x + 1 - df, y, z + 1)); // vIndex + 1
                                vertices.Add(new Vector3(x + 1 - df, y + 1, z)); // vIndex + 2
                                vertices.Add(new Vector3(x + 1 - df, y + 1, z + 1)); // vIndex + 3
                            }
                            int[] newTriangles = { 0, 2, 1, 1, 2, 3 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Constants.Resource[blocks[x, y, z], 1];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            Vector2 tileOffset = new Vector2((float)UVx * UVrX, (float)UVy * UVrY);

                            uvs.Add( new Vector2( 0f,  0f) + tileOffset );
                            uvs.Add( new Vector2(UVrX,  0f) + tileOffset );
                            uvs.Add( new Vector2( 0f, UVrY) + tileOffset );
                            uvs.Add( new Vector2(UVrX, UVrY) + tileOffset );
                        }

                        // LEFT
                        if (x == 0 || Constants.isTransparent(blocks[x - 1, y, z])) {
                            int vIndex = vertices.Count;
                            if (!isSmall) {
                                vertices.Add(new Vector3(x, y, z)); // vIndex
                                vertices.Add(new Vector3(x, y, z + 1)); // vIndex + 1
                                vertices.Add(new Vector3(x, y + 1, z)); // vIndex + 2
                                vertices.Add(new Vector3(x, y + 1, z + 1)); // vIndex + 3
                            } else {
                                vertices.Add(new Vector3(x + df, y, z)); // vIndex
                                vertices.Add(new Vector3(x + df, y, z + 1)); // vIndex + 1
                                vertices.Add(new Vector3(x + df, y + 1, z)); // vIndex + 2
                                vertices.Add(new Vector3(x + df, y + 1, z + 1)); // vIndex + 3
                            }
                            int[] newTriangles = { 0, 1, 2, 1, 3, 2 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Constants.Resource[blocks[x, y, z], 2];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            Vector2 tileOffset = new Vector2((float)UVx * UVrX, (float)UVy * UVrY);

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVrX, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVrY) + tileOffset);
                            uvs.Add(new Vector2(UVrX, UVrY) + tileOffset);
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
                            Vector2 tileOffset = new Vector2((float)UVx * UVrX, (float)UVy * UVrY);

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVrX, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVrY) + tileOffset);
                            uvs.Add(new Vector2(UVrX, UVrY) + tileOffset);
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
                            Vector2 tileOffset = new Vector2((float)UVx * UVrX, (float)UVy * UVrY);

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVrX, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVrY) + tileOffset);
                            uvs.Add(new Vector2(UVrX, UVrY) + tileOffset);
                        }

                        // BACK
                        if( z == Constants.chunkSizeZ - 1 || Constants.isTransparent(blocks[x, y, z + 1]) ) {
                            int vIndex = vertices.Count;
                            if (!isSmall) {
                                vertices.Add(new Vector3(x, y, z + 1)); // vIndex
                                vertices.Add(new Vector3(x + 1, y, z + 1)); // vIndex + 1
                                vertices.Add(new Vector3(x, y + 1, z + 1)); // vIndex + 2
                                vertices.Add(new Vector3(x + 1, y + 1, z + 1)); // vIndex + 3
                            } else {
                                vertices.Add(new Vector3(x, y, z + 1 - df)); // vIndex
                                vertices.Add(new Vector3(x + 1, y, z + 1 - df)); // vIndex + 1
                                vertices.Add(new Vector3(x, y + 1, z + 1 - df)); // vIndex + 2
                                vertices.Add(new Vector3(x + 1, y + 1, z + 1 - df)); // vIndex + 3
                            }
                            int[] newTriangles = { 0, 1, 2, 1, 3, 2 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Constants.Resource[blocks[x, y, z], 4];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            Vector2 tileOffset = new Vector2((float)UVx * UVrX, (float)UVy * UVrY);

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVrX, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVrY) + tileOffset);
                            uvs.Add(new Vector2(UVrX, UVrY) + tileOffset);
                        }

                        // FRONT
                        if( z == 0 || Constants.isTransparent(blocks[x, y, z - 1]) ) {
                            int vIndex = vertices.Count;
                            if(!isSmall) {
                                vertices.Add(new Vector3(x, y, z)); // vIndex
                                vertices.Add(new Vector3(x + 1, y, z)); // vIndex + 1
                                vertices.Add(new Vector3(x, y + 1, z)); // vIndex + 2
                                vertices.Add(new Vector3(x + 1, y + 1, z)); // vIndex + 3
                            } else {
                                vertices.Add(new Vector3(x, y, z + df)); // vIndex
                                vertices.Add(new Vector3(x + 1, y, z + df)); // vIndex + 1
                                vertices.Add(new Vector3(x, y + 1, z + df)); // vIndex + 2
                                vertices.Add(new Vector3(x + 1, y + 1, z + df)); // vIndex + 3
                            }
                            int[] newTriangles = { 0, 2, 1, 1, 2, 3 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Constants.Resource[blocks[x, y, z], 3];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            Vector2 tileOffset = new Vector2((float)UVx * UVrX, (float)UVy * UVrY);

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVrX, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVrY) + tileOffset);
                            uvs.Add(new Vector2(UVrX, UVrY) + tileOffset);
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

    private Mesh GenerateSolidMesh() {

        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        float UVrX = 1.0f / 16.0f, UVrY = 1.0f / 32.0f;

        for (int x = 0; x < Constants.chunkSizeX; x++) {
            for (int y = 0; y < Constants.chunkSizeY; y++) {
                for (int z = 0; z < Constants.chunkSizeZ; z++) {
                    if (!Constants.isNonSolid(blocks[x, y, z])) { // condition : block is solid

                        int texture;

                        // RIGHT
                        if (x == Constants.chunkSizeX - 1 || Constants.isNonSolid(blocks[x + 1, y, z])) {
                            int vIndex = vertices.Count;
                            vertices.Add(new Vector3(x + 1, y, z)); // vIndex
                            vertices.Add(new Vector3(x + 1, y, z + 1)); // vIndex + 1
                            vertices.Add(new Vector3(x + 1, y + 1, z)); // vIndex + 2
                            vertices.Add(new Vector3(x + 1, y + 1, z + 1)); // vIndex + 3
                            int[] newTriangles = { 0, 2, 1, 1, 2, 3 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Constants.Resource[blocks[x, y, z], 1];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            Vector2 tileOffset = new Vector2((float)UVx * UVrX, (float)UVy * UVrY);

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVrX, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVrY) + tileOffset);
                            uvs.Add(new Vector2(UVrX, UVrY) + tileOffset);
                        }

                        // LEFT
                        if (x == 0 || Constants.isNonSolid(blocks[x - 1, y, z])) {
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
                            Vector2 tileOffset = new Vector2((float)UVx * UVrX, (float)UVy * UVrY);

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVrX, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVrY) + tileOffset);
                            uvs.Add(new Vector2(UVrX, UVrY) + tileOffset);
                        }

                        // TOP
                        if (y == Constants.chunkSizeY - 1 || Constants.isNonSolid(blocks[x, y + 1, z])) {
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
                            Vector2 tileOffset = new Vector2((float)UVx * UVrX, (float)UVy * UVrY);

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVrX, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVrY) + tileOffset);
                            uvs.Add(new Vector2(UVrX, UVrY) + tileOffset);
                        }

                        // BOT
                        if (y == 0 || Constants.isNonSolid(blocks[x, y - 1, z])) {
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
                            Vector2 tileOffset = new Vector2((float)UVx * UVrX, (float)UVy * UVrY);

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVrX, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVrY) + tileOffset);
                            uvs.Add(new Vector2(UVrX, UVrY) + tileOffset);
                        }

                        // BACK
                        if (z == Constants.chunkSizeZ - 1 || Constants.isNonSolid(blocks[x, y, z + 1])) {
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
                            Vector2 tileOffset = new Vector2((float)UVx * UVrX, (float)UVy * UVrY);

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVrX, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVrY) + tileOffset);
                            uvs.Add(new Vector2(UVrX, UVrY) + tileOffset);
                        }

                        // FRONT
                        if (z == 0 || Constants.isNonSolid(blocks[x, y, z - 1])) {
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
                            Vector2 tileOffset = new Vector2((float)UVx * UVrX, (float)UVy * UVrY);

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVrX, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVrY) + tileOffset);
                            uvs.Add(new Vector2(UVrX, UVrY) + tileOffset);
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
