using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {

    private byte[,,] blocks = new byte[16, 128, 16];

    private Mesh mesh;
    private MeshFilter mf;
    private MeshRenderer mr;
    private MeshCollider mc;

    public Material tileset;
    
    // Texture Table                TOP   RIG   LEF   FRO   BAC   BOT
    private byte[,] Resource = { { 0xB4, 0xB4, 0xB4, 0xB4, 0xB4, 0xB4 },   // 0 Air
                                 { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },   // 1 Stone
                                 { 0x92, 0x03, 0x03, 0x03, 0x03, 0x02 },   // 2 Grass
                                 { 0x02, 0x02, 0x02, 0x02, 0x02, 0x02 },   // 3 Dirt
                                 { 0x10, 0x10, 0x10, 0x10, 0x10, 0x10 },   // 4 Cobblestone
                                 { 0x04, 0x04, 0x04, 0x04, 0x04, 0x04 },   // 5 Wood Plank
                                 { 0xB4, 0xB4, 0xB4, 0xB4, 0xB4, 0xB4 },   // 6 Sapling
                                 { 0x11, 0x11, 0x11, 0x11, 0x11, 0x11 },   // 7 Bedrock
                                 { 0xCF, 0xCF, 0xCF, 0xCF, 0xCF, 0xCF },   // 8 Flowing Water
                                 { 0xCF, 0xCD, 0xCD, 0xCD, 0xCD, 0xCF },   // 9 Still Water
                                 { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },   // 10 Flowing Lava
                                 { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },   // 11 Still Lava
                                 { 0x12, 0x12, 0x12, 0x12, 0x12, 0x12 },   // 12 Sand
                                 { 0x13, 0x13, 0x13, 0x13, 0x13, 0x13 },   // 13 Gravel
                                 { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 },   // 14 Gold Ore
                                 { 0x21, 0x21, 0x21, 0x21, 0x21, 0x21 },   // 15 Iron Ore
                                 { 0x22, 0x22, 0x22, 0x22, 0x22, 0x22 },   // 16 Coal Ore
                                 { 0x15, 0x14, 0x14, 0x14, 0x14, 0x15 },   // 17 Oak Wood
                               };

    // Hardness Table                 0      1      2      3      4      5      6      7      8      9
    private float[] Hardness = {
                                   0.1f,  7.5f,  0.9f,  0.9f, 10.0f,  5.0f, -1.0f, -1.0f, -1.0f, -1.0f,  // 0
                                  -1.0f, -1.0f, 0.75f,  0.9f, 15.0f, 15.0f, 15.0f,  3.0f                 // 1 
                               };

    // Transparent Blocks
    private byte[] transparentBlocks = { 0x00, 0x06 };
    private bool isTransparent(byte b) {
        bool ret = false;
        for(int i = 0; i < transparentBlocks.Length; i++) {
            if (b == transparentBlocks[i]) { ret = true; break; }
        }
        return ret;
    }

    // Selection Handling Variables
    private bool selected = false;
    private int selectedX, selectedY, selectedZ;
    private float selectedTime = 0.0f;
    private int phase;

    // Placement Handling Variables
    private int placeX, placeY, placeZ;
    private byte inventorySelected = 0x05;

    private GameObject crackingEffect;
    private MeshFilter _mf;
    private MeshRenderer _mr;

    public void setSelected(bool selected, Vector3 hitPoint) {

        this.selected = selected;
        if (!selected) {
            selectedX = -1;
            selectedY = -1;
            selectedZ = -1;
            selectedTime = 0.0f;
            phase = 0;
            if (crackingEffect != null) Destroy(crackingEffect);
        }

        // Create Gameobject to handle cracking effect
        if(selected) {
            if (CalculateSelectedBlock(hitPoint)) {
                if (Hardness[blocks[selectedX, selectedY, selectedZ]] > 0.0f) {
                    if (crackingEffect != null) Destroy(crackingEffect);

                    selectedTime = 0.0f;
                    crackingEffect = new GameObject();
                    crackingEffect.transform.position = transform.position;
                    _mf = crackingEffect.AddComponent<MeshFilter>();
                    _mr = crackingEffect.AddComponent<MeshRenderer>();
                    _mr.material = tileset;

                    _mf.mesh = GenerateCrackMesh();
                }
            }
        }

    }

    public void PlaceBlock(Vector3 hitPoint) {
        CalculatePlacingBlock(hitPoint);
        blocks[placeX, placeY, placeZ] = inventorySelected;
        Render();
    }

    private void Start() {
        
        // Test Generation
        for(int x = 0; x < 16; x++) {
            for(int z = 0; z < 16; z++) {
                blocks[x, 0, z] = 0x2;
            }
        }
        
        for(byte i = 0x1; i <= 0x11; i++) {
            blocks[i & 0x0F, 1, i >> 4] = i;
        }

        Render();

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

    private void Update() {
        
        // Handle Cracking
        if (selected && Hardness[blocks[selectedX, selectedY, selectedZ]] > 0.0f) {
            selectedTime += Time.deltaTime;
            int curPhase = Mathf.Min(9, Mathf.FloorToInt(selectedTime * 10.0f / Hardness[blocks[selectedX, selectedY, selectedZ]]));
            if (curPhase != phase) {
                phase = curPhase;
                _mf.mesh = GenerateCrackMesh();
            }

            if(selectedTime > Hardness[blocks[selectedX, selectedY, selectedZ]]) {
                // Block Delete Operation
                blocks[selectedX, selectedY, selectedZ] = 0x0;
                Render();
                setSelected(false, Vector3.zero);
            }
        }

    }

    private void CalculatePlacingBlock(Vector3 hitPoint) {

        Vector3 relativeHitPoint = hitPoint - transform.position;
        float x = relativeHitPoint.x;
        float y = relativeHitPoint.y;
        float z = relativeHitPoint.z;

        if (Mathf.Abs(x - Mathf.Round(x)) < 1e-4) {

            placeY = Mathf.FloorToInt(y);
            placeZ = Mathf.FloorToInt(z);

            int indexX = Mathf.RoundToInt(x);
            if (indexX == 0) placeX = 0;
            else if (indexX == 16) placeX = 15;
            else placeX = blocks[indexX, placeY, placeZ] != 0x0 ? indexX - 1 : indexX;

        } else if (Mathf.Abs(y - Mathf.Round(y)) < 1e-4) {

            placeX = Mathf.FloorToInt(x);
            placeZ = Mathf.FloorToInt(z);

            int indexY = Mathf.RoundToInt(y);
            if (indexY == 0) placeY = 0;
            else if (indexY == 16) placeY = 15;
            else placeY = blocks[placeX, indexY, placeZ] != 0x0 ? indexY - 1 : indexY;

        } else if (Mathf.Abs(z - Mathf.Round(z)) < 1e-4) {

            placeX = Mathf.FloorToInt(x);
            placeY = Mathf.FloorToInt(y);

            int indexZ = Mathf.RoundToInt(z);
            if (indexZ == 0) placeZ = 0;
            else if (indexZ == 16) placeZ = 15;
            else placeZ = blocks[placeX, placeY, indexZ] != 0x0 ? indexZ - 1 : indexZ;

        }

    }

    private bool CalculateSelectedBlock(Vector3 hitPoint) {

        Vector3 relativeHitPoint = hitPoint - transform.position;
        float x = relativeHitPoint.x;
        float y = relativeHitPoint.y;
        float z = relativeHitPoint.z;

        int _selectedX = -2, _selectedY = -2, _selectedZ = -2;

        if (Mathf.Abs(x - Mathf.Round(x)) < 1e-4 ) {

            _selectedY = Mathf.FloorToInt(y);
            _selectedZ = Mathf.FloorToInt(z);

            int indexX = Mathf.RoundToInt(x);
            if (indexX == 0) _selectedX = 0;
            else if (indexX == 16) _selectedX = 15;
            else _selectedX = blocks[indexX, _selectedY, _selectedZ] == 0x0 ? indexX - 1 : indexX;

        } else if (Mathf.Abs(y - Mathf.Round(y)) < 1e-4) {

            _selectedX = Mathf.FloorToInt(x);
            _selectedZ = Mathf.FloorToInt(z);

            int indexY = Mathf.RoundToInt(y);
            if (indexY == 0) _selectedY = 0;
            else if (indexY == 16) _selectedY = 15;
            else _selectedY = blocks[_selectedX, indexY, _selectedZ] == 0x0 ? indexY - 1 : indexY;

        } else if (Mathf.Abs(z - Mathf.Round(z)) < 1e-4) {

            _selectedX = Mathf.FloorToInt(x);
            _selectedY = Mathf.FloorToInt(y);

            int indexZ = Mathf.RoundToInt(z);
            if (indexZ == 0) _selectedZ = 0;
            else if (indexZ == 16) _selectedZ = 15;
            else _selectedZ = blocks[_selectedX, _selectedY, indexZ] == 0x0 ? indexZ - 1 : indexZ;

        }

        bool ret = (selectedX != _selectedX) || (selectedY != _selectedY) || (selectedZ != _selectedZ);

        selectedX = _selectedX;
        selectedY = _selectedY;
        selectedZ = _selectedZ;

        return ret;

    }

    public void Render() {

        if (mf == null) {
            mf = gameObject.AddComponent<MeshFilter>();
        }
        if (mr == null) {
            mr = gameObject.AddComponent<MeshRenderer>();
        }
        if (mc == null) {
            mc = gameObject.AddComponent<MeshCollider>();
        }

        mesh = GenerateMesh();

        mf.mesh = mesh;
        mc.sharedMesh = mesh;
        mr.material = tileset;

    }

    private Mesh GenerateCrackMesh() {

        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        int x = selectedX, y = selectedY, z = selectedZ;

        Vector3 pivot = x * Vector3.right + y * Vector3.up + z * Vector3.forward;

        int texture = 240 + phase;
        int UVx = texture & 0xF;
        int UVy = ~(texture >> 4);
        float UVr = 1.0f / 16.0f;
        Vector2 tileOffset = new Vector2((float)UVx, (float)UVy) * UVr;

        // RIGHT
        if (x == 15 || isTransparent(blocks[x + 1, y, z]) ) {
            int vIndex = vertices.Count;
            vertices.Add(pivot + new Vector3(1.01f, -0.01f, -0.01f)); // vIndex
            vertices.Add(pivot + new Vector3(1.01f, -0.01f, 1.01f)); // vIndex + 1
            vertices.Add(pivot + new Vector3(1.01f, 1.01f, -0.01f)); // vIndex + 2
            vertices.Add(pivot + new Vector3(1.01f, 1.01f, 1.01f)); // vIndex + 3
            int[] newTriangles = { 0, 2, 1, 1, 2, 3 };
            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
            triangles.AddRange(newTriangles);

            uvs.Add(new Vector2(0f, 0f) + tileOffset);
            uvs.Add(new Vector2(UVr, 0f) + tileOffset);
            uvs.Add(new Vector2(0f, UVr) + tileOffset);
            uvs.Add(new Vector2(UVr, UVr) + tileOffset);
        }

        // LEFT
        if (x == 0 || isTransparent(blocks[x - 1, y, z])) {
            int vIndex = vertices.Count;
            vertices.Add(pivot + new Vector3(-0.01f, -0.01f, -0.01f)); // vIndex
            vertices.Add(pivot + new Vector3(-0.01f, -0.01f, 1.01f)); // vIndex + 1
            vertices.Add(pivot + new Vector3(-0.01f, 1.01f, -0.01f)); // vIndex + 2
            vertices.Add(pivot + new Vector3(-0.01f, 1.01f, 1.01f)); // vIndex + 3
            int[] newTriangles = { 0, 1, 2, 1, 3, 2 };
            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
            triangles.AddRange(newTriangles);

            uvs.Add(new Vector2(0f, 0f) + tileOffset);
            uvs.Add(new Vector2(UVr, 0f) + tileOffset);
            uvs.Add(new Vector2(0f, UVr) + tileOffset);
            uvs.Add(new Vector2(UVr, UVr) + tileOffset);
        }

        // TOP
        if (y == 127 || isTransparent(blocks[x, y + 1, z])) {
            int vIndex = vertices.Count;
            vertices.Add(pivot + new Vector3(-0.01f, 1.01f, -0.01f)); // vIndex
            vertices.Add(pivot + new Vector3(1.01f, 1.01f, -0.01f)); // vIndex + 1
            vertices.Add(pivot + new Vector3(-0.01f, 1.01f, 1.01f)); // vIndex + 2
            vertices.Add(pivot + new Vector3(1.01f, 1.01f, 1.01f)); // vIndex + 3
            int[] newTriangles = { 0, 2, 1, 1, 2, 3 };
            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
            triangles.AddRange(newTriangles);

            uvs.Add(new Vector2(0f, 0f) + tileOffset);
            uvs.Add(new Vector2(UVr, 0f) + tileOffset);
            uvs.Add(new Vector2(0f, UVr) + tileOffset);
            uvs.Add(new Vector2(UVr, UVr) + tileOffset);
        }

        // BOT
        if (y == 0 || isTransparent(blocks[x, y - 1, z])) {
            int vIndex = vertices.Count;
            vertices.Add(pivot + new Vector3(-0.01f, -0.01f, -0.01f)); // vIndex
            vertices.Add(pivot + new Vector3(1.01f, -0.01f, -0.01f)); // vIndex + 1
            vertices.Add(pivot + new Vector3(-0.01f, -0.01f, 1.01f)); // vIndex + 2
            vertices.Add(pivot + new Vector3(1.01f, -0.01f, 1.01f)); // vIndex + 3
            int[] newTriangles = { 0, 1, 2, 1, 3, 2 };
            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
            triangles.AddRange(newTriangles);

            uvs.Add(new Vector2(0f, 0f) + tileOffset);
            uvs.Add(new Vector2(UVr, 0f) + tileOffset);
            uvs.Add(new Vector2(0f, UVr) + tileOffset);
            uvs.Add(new Vector2(UVr, UVr) + tileOffset);
        }

        // BACK
        if (z == 15 || isTransparent(blocks[x, y, z + 1])) {
            int vIndex = vertices.Count;
            vertices.Add(pivot + new Vector3(-0.01f, -0.01f, 1.01f)); // vIndex
            vertices.Add(pivot + new Vector3(1.01f, -0.01f, 1.01f)); // vIndex + 1
            vertices.Add(pivot + new Vector3(-0.01f, 1.01f, 1.01f)); // vIndex + 2
            vertices.Add(pivot + new Vector3(1.01f, 1.01f, 1.01f)); // vIndex + 3
            int[] newTriangles = { 0, 1, 2, 1, 3, 2 };
            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
            triangles.AddRange(newTriangles);

            uvs.Add(new Vector2(0f, 0f) + tileOffset);
            uvs.Add(new Vector2(UVr, 0f) + tileOffset);
            uvs.Add(new Vector2(0f, UVr) + tileOffset);
            uvs.Add(new Vector2(UVr, UVr) + tileOffset);
        }

        // FRONT
        if (z == 0 || isTransparent(blocks[x, y, z - 1])) {
            int vIndex = vertices.Count;
            vertices.Add(pivot + new Vector3(-0.01f, -0.01f, -0.01f)); // vIndex
            vertices.Add(pivot + new Vector3(1.01f, -0.01f, -0.01f)); // vIndex + 1
            vertices.Add(pivot + new Vector3(-0.01f, 1.01f, -0.01f)); // vIndex + 2
            vertices.Add(pivot + new Vector3(1.01f, 1.01f, -0.01f)); // vIndex + 3
            int[] newTriangles = { 0, 2, 1, 1, 2, 3 };
            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
            triangles.AddRange(newTriangles);

            uvs.Add(new Vector2(0f, 0f) + tileOffset);
            uvs.Add(new Vector2(UVr, 0f) + tileOffset);
            uvs.Add(new Vector2(0f, UVr) + tileOffset);
            uvs.Add(new Vector2(UVr, UVr) + tileOffset);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        return mesh;

    }

    private Mesh GenerateMesh() {

        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for(int x = 0; x < 16; x++) {
            for(int y = 0; y < 128; y++) {
                for(int z = 0; z < 16; z++) {
                    if( !isTransparent(blocks[x, y, z]) ) { // condition : block is solid

                        Vector3 pivot = x * Vector3.right + y * Vector3.up + z * Vector3.forward;
                        int texture;
                        
                        // RIGHT
                        if( x == 15 || isTransparent(blocks[x + 1, y, z]) ) {
                            int vIndex = vertices.Count;
                            vertices.Add(pivot + new Vector3(1, 0, 0)); // vIndex
                            vertices.Add(pivot + new Vector3(1, 0, 1)); // vIndex + 1
                            vertices.Add(pivot + new Vector3(1, 1, 0)); // vIndex + 2
                            vertices.Add(pivot + new Vector3(1, 1, 1)); // vIndex + 3
                            int[] newTriangles = { 0, 2, 1, 1, 2, 3 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Resource[blocks[x, y, z], 1];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            float UVr = 1.0f / 16.0f;
                            Vector2 tileOffset = new Vector2((float)UVx, (float)UVy) * UVr;

                            uvs.Add( new Vector2( 0f,  0f) + tileOffset );
                            uvs.Add( new Vector2(UVr,  0f) + tileOffset );
                            uvs.Add( new Vector2( 0f, UVr) + tileOffset );
                            uvs.Add( new Vector2(UVr, UVr) + tileOffset );
                        }

                        // LEFT
                        if ( x == 0 || isTransparent(blocks[x - 1, y, z]) ) {
                            int vIndex = vertices.Count;
                            vertices.Add(pivot + new Vector3(0, 0, 0)); // vIndex
                            vertices.Add(pivot + new Vector3(0, 0, 1)); // vIndex + 1
                            vertices.Add(pivot + new Vector3(0, 1, 0)); // vIndex + 2
                            vertices.Add(pivot + new Vector3(0, 1, 1)); // vIndex + 3
                            int[] newTriangles = { 0, 1, 2, 1, 3, 2 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Resource[blocks[x, y, z], 2];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            float UVr = 1.0f / 16.0f;
                            Vector2 tileOffset = new Vector2((float)UVx, (float)UVy) * UVr;

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVr, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVr) + tileOffset);
                            uvs.Add(new Vector2(UVr, UVr) + tileOffset);
                        }

                        // TOP
                        if( y == 127 || isTransparent(blocks[x, y + 1, z]) ) {
                            int vIndex = vertices.Count;
                            vertices.Add(pivot + new Vector3(0, 1, 0)); // vIndex
                            vertices.Add(pivot + new Vector3(1, 1, 0)); // vIndex + 1
                            vertices.Add(pivot + new Vector3(0, 1, 1)); // vIndex + 2
                            vertices.Add(pivot + new Vector3(1, 1, 1)); // vIndex + 3
                            int[] newTriangles = { 0, 2, 1, 1, 2, 3 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Resource[blocks[x, y, z], 0];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            float UVr = 1.0f / 16.0f;
                            Vector2 tileOffset = new Vector2((float)UVx, (float)UVy) * UVr;

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVr, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVr) + tileOffset);
                            uvs.Add(new Vector2(UVr, UVr) + tileOffset);
                        }

                        // BOT
                        if( y == 0 || isTransparent(blocks[x, y - 1, z]) ) {
                            int vIndex = vertices.Count;
                            vertices.Add(pivot + new Vector3(0, 0, 0)); // vIndex
                            vertices.Add(pivot + new Vector3(1, 0, 0)); // vIndex + 1
                            vertices.Add(pivot + new Vector3(0, 0, 1)); // vIndex + 2
                            vertices.Add(pivot + new Vector3(1, 0, 1)); // vIndex + 3
                            int[] newTriangles = { 0, 1, 2, 1, 3, 2 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Resource[blocks[x, y, z], 5];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            float UVr = 1.0f / 16.0f;
                            Vector2 tileOffset = new Vector2((float)UVx, (float)UVy) * UVr;

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVr, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVr) + tileOffset);
                            uvs.Add(new Vector2(UVr, UVr) + tileOffset);
                        }

                        // BACK
                        if( z == 15 || isTransparent(blocks[x, y, z + 1]) ) {
                            int vIndex = vertices.Count;
                            vertices.Add(pivot + new Vector3(0, 0, 1)); // vIndex
                            vertices.Add(pivot + new Vector3(1, 0, 1)); // vIndex + 1
                            vertices.Add(pivot + new Vector3(0, 1, 1)); // vIndex + 2
                            vertices.Add(pivot + new Vector3(1, 1, 1)); // vIndex + 3
                            int[] newTriangles = { 0, 1, 2, 1, 3, 2 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Resource[blocks[x, y, z], 4];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            float UVr = 1.0f / 16.0f;
                            Vector2 tileOffset = new Vector2((float)UVx, (float)UVy) * UVr;

                            uvs.Add(new Vector2(0f, 0f) + tileOffset);
                            uvs.Add(new Vector2(UVr, 0f) + tileOffset);
                            uvs.Add(new Vector2(0f, UVr) + tileOffset);
                            uvs.Add(new Vector2(UVr, UVr) + tileOffset);
                        }

                        // FRONT
                        if( z == 0 || isTransparent(blocks[x, y, z - 1]) ) {
                            int vIndex = vertices.Count;
                            vertices.Add(pivot + new Vector3(0, 0, 0)); // vIndex
                            vertices.Add(pivot + new Vector3(1, 0, 0)); // vIndex + 1
                            vertices.Add(pivot + new Vector3(0, 1, 0)); // vIndex + 2
                            vertices.Add(pivot + new Vector3(1, 1, 0)); // vIndex + 3
                            int[] newTriangles = { 0, 2, 1, 1, 2, 3 };
                            for (int i = 0; i < 6; i++) newTriangles[i] += vIndex;
                            triangles.AddRange(newTriangles);

                            texture = Resource[blocks[x, y, z], 3];
                            int UVx = texture & 0xF;
                            int UVy = ~(texture >> 4);
                            float UVr = 1.0f / 16.0f;
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
