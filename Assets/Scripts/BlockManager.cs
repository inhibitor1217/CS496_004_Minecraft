using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour {

    // External Storage Path
    public string mapPath = "";

    // Block Crack Particle Effect
    public GameObject crackParticlePrefab;
    private GameObject particleObj;
    private GameObject crackingEffect;
    private MeshFilter _mf;
    private MeshRenderer _mr;

    // Selection Handling Variables
    private bool selected = false;
    private int selectedX, selectedY, selectedZ;
    private float selectedTime = 0.0f;
    private int phase;

    // Placement Handling Variables
    private int placeX, placeY, placeZ;
    private byte inventorySelected = 0x05;

    // Block Management
    private int playerChunkX, playerChunkY;
    private int chunkOffsetX = 0, chunkOffsetY = 0;
    private bool initializedChunkArray = false;
    private Chunk[,] chunks;
    public GameObject chunkPrefab;

    // Block Texture
    public Material tileset;

    private void Awake() {

        // Initialize Arrays
        chunks = new Chunk[Constants.chunkArraySize, Constants.chunkArraySize];

        mapPath = MainSceneLoadInfo.info.mapPath;
        if(MainSceneLoadInfo.info.shouldGenerateMap) {
            GenerateTerrain();
        }

    }

    private void OnDestroy() {
        for(int i = 0; i < Constants.chunkArraySize; i++) {
            for(int j = 0; j < Constants.chunkArraySize; j++) {
                if(chunks[i, j] != null) {
                    SaveChunk(chunks[i, j]);
                }
            }
        }
    }

    private void Update() {

        // Handle Cracking
        if (selected && Constants.Hardness[getBlock(selectedX, selectedY, selectedZ)] > 0.0f) {
            selectedTime += Time.deltaTime;
            int curPhase = Mathf.Min(9, Mathf.FloorToInt(selectedTime * 10.0f / Constants.Hardness[getBlock(selectedX, selectedY, selectedZ)]));
            if (curPhase != phase) {
                phase = curPhase;
                _mf.mesh = GenerateCrackMesh();
            }

            if (selectedTime > Constants.Hardness[getBlock(selectedX, selectedY, selectedZ)]) {
                // Block Delete Operation
                setBlockAndRender(selectedX, selectedY, selectedZ, 0x00);
                setSelected(false);
            }
        }

    }

    private int getChunkX(int x) {
        int chunkX = x >= 0 ? (x >> 4) : (x >> 4) | 0xF000;
        return (chunkOffsetX + chunkX - playerChunkX + Constants.chunkArraySize) & Constants.chunkArrayMask;
    }

    private int getChunkY(int z) {
        int chunkY = z >= 0 ? (z >> 4) : (z >> 4) | 0xF000;
        return (chunkOffsetY + chunkY - playerChunkY + Constants.chunkArraySize) & Constants.chunkArrayMask;
    }

    private byte getBlock(int x, int y, int z) {
        return chunks[getChunkX(x), getChunkY(z)].blocks[x & 0x0F, y & 0x7F, z & 0x0F];
    }

    // This method is only used to initialize terrain
    private void setBlockInChunk(int x, int y, int z, byte block, Chunk chunk) {
        if(0 <= x && x < Constants.chunkSizeX 
        && 0 <= y && y < Constants.chunkSizeY 
        && 0 <= z && z < Constants.chunkSizeZ) {
            chunk.blocks[x, y, z] = block;
        }
    }

    private void setBlock(int x, int y, int z, byte block) {
        chunks[getChunkX(x), getChunkY(z)].blocks[x & 0x0F, y & 0x7F, z & 0x0F] = block;
    }

    private void setBlockAndRender(int x, int y, int z, byte block) {
        setBlock(x, y, z, block);
        chunks[getChunkX(x), getChunkY(z)].Render();
    }

    public void GenerateTerrain() {
    
        // TODO : use priority queue for sequencing chunk rendering
        for (int i = -8; i < 8; i++) {
            for (int j = -8; j < 8; j++) {

                Chunk chunk = InitializeChunk(i, j);

                for (int x = 0; x < 16; x++) {
                    for (int z = 0; z < 16; z++) {
                        for (int y = 0; y < 64; y++) {
                            if (y == 0)       setBlockInChunk(x, y, z, 0x07, chunk);
                            else if (y == 63) setBlockInChunk(x, y, z, 0x02, chunk);
                            else              setBlockInChunk(x, y, z, 0x03, chunk);
                        }
                    }
                }
                chunk.Render();
                SaveChunk(chunk);

                Destroy(chunk.gameObject);

            }
        }

    }

    public Chunk InitializeChunk(int chunkX, int chunkY) {
        Chunk ret = Instantiate(chunkPrefab, Vector3.right * Constants.chunkSizeX * chunkX + Vector3.forward * Constants.chunkSizeZ * chunkY, Quaternion.identity).GetComponent<Chunk>();
        ret.chunkX = chunkX;
        ret.chunkY = chunkY;
        return ret;
    }

    public Chunk LoadChunk(int chunkX, int chunkY) {

        string id = Chunk.getId(chunkX, chunkY);
        string path = mapPath + "/chunks/" + id + ".dat";

        if(File.Exists(path)) {
            
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);
            ChunkData data = (ChunkData) binaryFormatter.Deserialize(file);
            file.Close();

            Chunk ret = Instantiate(chunkPrefab, Vector3.right * Constants.chunkSizeX * chunkX + Vector3.forward * Constants.chunkSizeZ * chunkY, Quaternion.identity).GetComponent<Chunk>();
            ret.chunkX = data.chunkX;
            ret.chunkY = data.chunkY;
            
            ret.blocks = data.blockData;
            ret.Render();

            return ret;

        }

        return null;

    }

    public void SaveChunk(Chunk chunk) {

        if (chunk == null) return;

        ChunkData data = chunk.makeChunkData();
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        if(!Directory.Exists(mapPath + "/chunks")) {
            Directory.CreateDirectory(mapPath + "/chunks");
        }

        FileStream file;
        string path = mapPath + "/chunks/" + data.id + ".dat";

        file = File.Create(path);

        binaryFormatter.Serialize(file, data);
        file.Close();
        
    }

    public void setSelected(bool selected, Vector3 hitPoint, Vector3 forward, Vector3 up) {

        this.selected = selected;

        // Create Gameobject to handle cracking effect
        if (selected) {
            if (CalculateSelectedBlock(hitPoint)) {
                if (Constants.Hardness[getBlock(selectedX, selectedY, selectedZ)] > 0.0f) { // If block is breakable
                    if (crackingEffect != null) Destroy(crackingEffect);

                    selectedTime = 0.0f;
                    crackingEffect = new GameObject();
                    _mf = crackingEffect.AddComponent<MeshFilter>();
                    _mr = crackingEffect.AddComponent<MeshRenderer>();
                    _mr.material = tileset;

                    _mf.mesh = GenerateCrackMesh();
                }

                if (particleObj != null) Destroy(particleObj);
                particleObj = Instantiate(crackParticlePrefab, hitPoint, Quaternion.LookRotation(forward, up));
                byte texture = Constants.Resource[getBlock(selectedX, selectedY, selectedZ), 1];
                int UVx = texture & 0xF;
                int UVy = ~(texture >> 4);
                particleObj.GetComponent<ParticleSystemRenderer>().material.SetTextureOffset("_MainTex", new Vector2((float)UVx / 16.0f, (float)UVy / 16.0f));

            }
        }

    }

    public void setSelected(bool selected) {

        this.selected = selected;

        if (!selected) {
            selectedX = -1;
            selectedY = -1;
            selectedZ = -1;
            selectedTime = 0.0f;
            phase = 0;
            if (crackingEffect != null) Destroy(crackingEffect);
            if (particleObj != null) Destroy(particleObj);
        }

    }

    public void PlaceBlock(Vector3 hitPoint, Vector3 playerPosition) {
        CalculatePlacingBlock(hitPoint);
        if (placeX != Mathf.FloorToInt(playerPosition.x)
            || (placeY != Mathf.FloorToInt(playerPosition.y) + 1 && placeY != Mathf.FloorToInt(playerPosition.y) + 2)
            || placeZ != Mathf.FloorToInt(playerPosition.z)) { // Do not place block where player stands
            setBlockAndRender(placeX, placeY, placeZ, inventorySelected);
        }
    }

    private void CalculatePlacingBlock(Vector3 hitPoint) {

        float x = hitPoint.x;
        float y = hitPoint.y;
        float z = hitPoint.z;

        if (Mathf.Abs(x - Mathf.Round(x)) < 1e-4) {

            placeY = Mathf.FloorToInt(y);
            placeZ = Mathf.FloorToInt(z);

            int indexX = Mathf.RoundToInt(x);
            placeX = getBlock(indexX, placeY, placeZ) != 0x0 ? indexX - 1 : indexX;

        } else if (Mathf.Abs(y - Mathf.Round(y)) < 1e-4) {

            placeX = Mathf.FloorToInt(x);
            placeZ = Mathf.FloorToInt(z);

            int indexY = Mathf.RoundToInt(y);
            placeY = getBlock(placeX, indexY, placeZ) != 0x0 ? indexY - 1 : indexY;

        } else if (Mathf.Abs(z - Mathf.Round(z)) < 1e-4) {

            placeX = Mathf.FloorToInt(x);
            placeY = Mathf.FloorToInt(y);

            int indexZ = Mathf.RoundToInt(z);
            placeZ = getBlock(placeX, placeY, indexZ) != 0x0 ? indexZ - 1 : indexZ;

        }

    }

    private bool CalculateSelectedBlock(Vector3 hitPoint) {

        float x = hitPoint.x;
        float y = hitPoint.y;
        float z = hitPoint.z;

        int _selectedX = -2, _selectedY = -2, _selectedZ = -2;

        if (Mathf.Abs(x - Mathf.Round(x)) < 1e-4) {

            _selectedY = Mathf.FloorToInt(y);
            _selectedZ = Mathf.FloorToInt(z);

            int indexX = Mathf.RoundToInt(x);
            _selectedX = getBlock(indexX, _selectedY, _selectedZ) == 0x0 ? indexX - 1 : indexX;

        } else if (Mathf.Abs(y - Mathf.Round(y)) < 1e-4) {

            _selectedX = Mathf.FloorToInt(x);
            _selectedZ = Mathf.FloorToInt(z);

            int indexY = Mathf.RoundToInt(y);
            _selectedY = getBlock(_selectedX, indexY, _selectedZ) == 0x0 ? indexY - 1 : indexY;

        } else if (Mathf.Abs(z - Mathf.Round(z)) < 1e-4) {

            _selectedX = Mathf.FloorToInt(x);
            _selectedY = Mathf.FloorToInt(y);

            int indexZ = Mathf.RoundToInt(z);
            _selectedZ = getBlock(_selectedX, _selectedY, indexZ) == 0x0 ? indexZ - 1 : indexZ;

        }

        bool ret = (selectedX != _selectedX) || (selectedY != _selectedY) || (selectedZ != _selectedZ);

        selectedX = _selectedX;
        selectedY = _selectedY;
        selectedZ = _selectedZ;

        return ret;

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
        if (Constants.isTransparent(getBlock(x + 1, y, z))) {
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
        if (Constants.isTransparent(getBlock(x - 1, y, z))) {
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
        if (Constants.isTransparent(getBlock(x, y + 1, z))) {
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
        if (Constants.isTransparent(getBlock(x, y - 1, z))) {
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
        if (Constants.isTransparent(getBlock(x, y, z + 1))) {
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
        if (Constants.isTransparent(getBlock(x, y, z - 1))) {
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

    public void UpdatePlayerPosition(Vector3 position) {

        int _playerChunkX = Mathf.FloorToInt(position.x / Constants.chunkSizeX);
        int _playerChunkY = Mathf.FloorToInt(position.z / Constants.chunkSizeZ);

        if(!initializedChunkArray) {
            playerChunkX = _playerChunkX;
            playerChunkY = _playerChunkY;
            InitializeChunkArray();
            return;
        }

        if(_playerChunkX == playerChunkX - 1) { // Moved -X
            
            for(int dy = -Constants.visibleChunkDistance; dy <= Constants.visibleChunkDistance; dy++) {
                
                chunks[(chunkOffsetX + Constants.chunkArraySize + (dy > 0 ? dy : -dy) - Constants.visibleChunkDistance - 1 ) & Constants.chunkArrayMask,
                       (chunkOffsetY + Constants.chunkArraySize + dy) & Constants.chunkArrayMask] = LoadChunk(_playerChunkX + (dy > 0 ? dy : -dy) - Constants.visibleChunkDistance, playerChunkY + dy);

                Chunk saveChunk = chunks[(chunkOffsetX - (dy > 0 ? dy : -dy) + Constants.visibleChunkDistance) & Constants.chunkArrayMask,
                                         (chunkOffsetY + Constants.chunkArraySize + dy) & Constants.chunkArrayMask];
                chunks[(chunkOffsetX - (dy > 0 ? dy : -dy) + Constants.visibleChunkDistance) & Constants.chunkArrayMask,
                       (chunkOffsetY + Constants.chunkArraySize + dy) & Constants.chunkArrayMask] = null;
                if (saveChunk != null) {
                    SaveChunk(saveChunk);
                    Destroy(saveChunk.gameObject);
                }

            }
            chunkOffsetX += Constants.chunkArraySize - 1;
            chunkOffsetX &= Constants.chunkArrayMask;

        } else if(_playerChunkX == playerChunkX + 1) { // Moved +X
        
            for (int dy = -Constants.visibleChunkDistance; dy <= Constants.visibleChunkDistance; dy++) {

                chunks[(chunkOffsetX + Constants.visibleChunkDistance - (dy > 0 ? dy : -dy) + 1) & Constants.chunkArrayMask,
                       (chunkOffsetY + Constants.chunkArraySize + dy) & Constants.chunkArrayMask] = LoadChunk(_playerChunkX - (dy > 0 ? dy : -dy) + Constants.visibleChunkDistance, playerChunkY + dy);

                Chunk saveChunk = chunks[(chunkOffsetX + Constants.chunkArraySize + (dy > 0 ? dy : -dy) - Constants.visibleChunkDistance) & Constants.chunkArrayMask,
                                         (chunkOffsetY + Constants.chunkArraySize + dy) & Constants.chunkArrayMask];
                chunks[(chunkOffsetX + Constants.chunkArraySize + (dy > 0 ? dy : -dy) - Constants.visibleChunkDistance) & Constants.chunkArrayMask,
                       (chunkOffsetY + Constants.chunkArraySize + dy) & Constants.chunkArrayMask] = null;
                if (saveChunk != null) {
                    SaveChunk(saveChunk);
                    Destroy(saveChunk.gameObject);
                }
            }
            chunkOffsetX += 1;
            chunkOffsetX &= Constants.chunkArrayMask;

        }

        playerChunkX = _playerChunkX;

        if (_playerChunkY == playerChunkY - 1) { // Moved -Z
            
            for(int dx = -Constants.visibleChunkDistance; dx <= Constants.visibleChunkDistance; dx++) {

                chunks[(chunkOffsetX + Constants.chunkArraySize + dx) & Constants.chunkArrayMask,
                       (chunkOffsetY + Constants.chunkArraySize + (dx > 0 ? dx : -dx) - Constants.visibleChunkDistance - 1) & Constants.chunkArrayMask] = LoadChunk(playerChunkX + dx, _playerChunkY + (dx > 0 ? dx : -dx) - Constants.visibleChunkDistance);

                Chunk saveChunk = chunks[(chunkOffsetX + Constants.chunkArraySize + dx) & Constants.chunkArrayMask,
                                         (chunkOffsetY + Constants.visibleChunkDistance - (dx > 0 ? dx : -dx)) & Constants.chunkArrayMask];
                chunks[(chunkOffsetX + Constants.chunkArraySize + dx) & Constants.chunkArrayMask,
                       (chunkOffsetY + Constants.visibleChunkDistance - (dx > 0 ? dx : -dx)) & Constants.chunkArrayMask] = null;
                if (saveChunk != null) {
                    SaveChunk(saveChunk);
                    Destroy(saveChunk.gameObject);
                }
            }
            chunkOffsetY += Constants.chunkArraySize - 1;
            chunkOffsetY &= Constants.chunkArrayMask;

        } else if(_playerChunkY == playerChunkY + 1) { // Moved +Z
            
            for (int dx = -Constants.visibleChunkDistance; dx <= Constants.visibleChunkDistance; dx++) {

                chunks[(chunkOffsetX + Constants.chunkArraySize + dx) & Constants.chunkArrayMask,
                       (chunkOffsetY + Constants.visibleChunkDistance - (dx > 0 ? dx : -dx) + 1) & Constants.chunkArrayMask] = LoadChunk(playerChunkX + dx, _playerChunkY - (dx > 0 ? dx : -dx) + Constants.visibleChunkDistance);

                Chunk saveChunk = chunks[(chunkOffsetX + Constants.chunkArraySize + dx) & Constants.chunkArrayMask,
                                         (chunkOffsetY + Constants.chunkArraySize + (dx > 0 ? dx : -dx) - Constants.visibleChunkDistance) & Constants.chunkArrayMask];
                chunks[(chunkOffsetX + Constants.chunkArraySize + dx) & Constants.chunkArrayMask,
                       (chunkOffsetY + Constants.chunkArraySize + (dx > 0 ? dx : -dx) - Constants.visibleChunkDistance) & Constants.chunkArrayMask] = null;
                if (saveChunk != null) {
                    SaveChunk(saveChunk);
                    Destroy(saveChunk.gameObject);
                }
            }
            chunkOffsetY += 1;
            chunkOffsetY &= Constants.chunkArrayMask;

        }

        playerChunkY = _playerChunkY;

    }

    private void InitializeChunkArray() {
        for(int dx = -Constants.visibleChunkDistance; dx <= Constants.visibleChunkDistance; dx++) {
            for(int dy = -Constants.visibleChunkDistance; dy <= Constants.visibleChunkDistance; dy++) {
                if ((dx > 0 ? dx : -dx) + (dy > 0 ? dy : -dy) > Constants.visibleChunkDistance) continue;
                chunks[(dx + chunkOffsetX + Constants.chunkArraySize) & Constants.chunkArrayMask, 
                       (dy + chunkOffsetY + Constants.chunkArraySize) & Constants.chunkArrayMask] = LoadChunk(playerChunkX + dx, playerChunkY + dy);
            }
        }
        initializedChunkArray = true;
    }

}
