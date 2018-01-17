using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

    public byte[,] terrain;
    public byte[,] blocks;
    public string mapPath;

    private static int biomeMax = 10;
    public static float weightOcean = 1.2f;
    public static float treefill = 0.1f;
    public static float cactusfill = 0.0025f;
    public static float grassfill = 0.25f;
    public static float flowerfill = 0.002f;

    public GameObject[] chunkTerrainGenPrefab = new GameObject[5];

    private ChunkTerrain[,] chunkTerrains;

    public void Generate(int mapSizeX, int mapSizeY) {

        // initialize
        blocks = new byte[mapSizeX * Constants.chunkSizeX, mapSizeY * Constants.chunkSizeZ];

        float offsetX = Random.Range(0.0f, 10000.0f);
        float offsetY = Random.Range(0.0f, 10000.0f);

        chunkTerrains = new ChunkTerrain[mapSizeX, mapSizeY];

        GenerateBiome(mapSizeX, mapSizeY);

        for (int i = -mapSizeX / 2; i < mapSizeX / 2; i++) {
            for (int j = -mapSizeY / 2; j < mapSizeY / 2; j++) {
                int biome = terrain[i + mapSizeX / 2, j + mapSizeY / 2];
                chunkTerrains[i + mapSizeX / 2, j + mapSizeY / 2] = Instantiate(chunkTerrainGenPrefab[biome]).GetComponent<ChunkTerrain>();
                chunkTerrains[i + mapSizeX / 2, j + mapSizeY / 2].offsetX = offsetX;
                chunkTerrains[i + mapSizeX / 2, j + mapSizeY / 2].offsetY = offsetY;
            }
        }

        for(int i= -mapSizeX / 2; i < mapSizeX / 2; i++) {
            for (int j = -mapSizeY / 2; j < mapSizeY / 2; j++) {
                for (int x = 0; x < Constants.chunkSizeX; x++) {
                    for (int z = 0; z < Constants.chunkSizeZ; z++) {

                        int dx = 0, dy = 0;
                        if (x < Constants.chunkSizeX / 2) {
                            if (i > -Constants.mapSizeX / 2) {
                                dx = -1;
                            }
                        } else {
                            if (i < Constants.mapSizeX / 2 - 1) {
                                dx = 1;
                            }
                        }
                        if (z < Constants.chunkSizeZ / 2) {
                            if (j > -Constants.mapSizeY / 2) {
                                dy = -1;
                            }
                        } else {
                            if (j < Constants.mapSizeY / 2 - 1) {
                                dy = 1;
                            }
                        }

                        float weightX = Mathf.Abs(x + 0.5f - Constants.chunkSizeX / 2) / (float)Constants.chunkSizeX;
                        float weightY = Mathf.Abs(z + 0.5f - Constants.chunkSizeZ / 2) / (float)Constants.chunkSizeZ;
                        
                        int xCoor = (i + mapSizeX / 2) * Constants.chunkSizeX + x;
                        int yCoor = (j + mapSizeY / 2) * Constants.chunkSizeZ + z;
                        if (weightX > 0.3f || weightY > 0.3f) {
                            if (Random.Range(0.0f, 1.0f) < weightX) {
                                if (Random.Range(0.0f, 1.0f) < weightY) {
                                    blocks[xCoor, yCoor] = terrain[i + mapSizeX / 2 + dx, j + mapSizeY / 2 + dy];
                                } else {
                                    blocks[xCoor, yCoor] = terrain[i + mapSizeX / 2 + dx, j + mapSizeY / 2];
                                }
                            } else {
                                if (Random.Range(0.0f, 1.0f) < weightY) {
                                    blocks[xCoor, yCoor] = terrain[i + mapSizeX / 2, j + mapSizeY / 2 + dy];
                                } else {
                                    blocks[xCoor, yCoor] = terrain[i + mapSizeX / 2, j + mapSizeY / 2];
                                }
                            }
                        } else {
                            blocks[xCoor, yCoor] = terrain[i + mapSizeX / 2, j + mapSizeY / 2];
                        }
                    }
                }
            }
        }

        SmoothTerrains();

        for (int i = -mapSizeX / 2; i < mapSizeX / 2; i++) {
            for(int j = -mapSizeY / 2; j < mapSizeY / 2; j++) {

                ChunkData data = new ChunkData();
                data.id = i.ToString("X8") + j.ToString("X8");
                data.chunkX = i;
                data.chunkY = j;

                byte[,,] arr = new byte[Constants.chunkSizeX, Constants.chunkSizeY, Constants.chunkSizeZ];
                for(int x = 0; x < Constants.chunkSizeX; x++) {
                    for (int z = 0; z < Constants.chunkSizeZ; z++) {

                        int dx = 0, dy = 0;
                        if(x < Constants.chunkSizeX / 2) {
                            if(i > -Constants.mapSizeX / 2) {
                                dx = -1;
                            }
                        } else {
                            if(i < Constants.mapSizeX / 2 - 1) {
                                dx = 1;
                            }
                        }
                        if (z < Constants.chunkSizeZ / 2) {
                            if (j > -Constants.mapSizeY / 2) {
                                dy = -1;
                            }
                        } else {
                            if (j < Constants.mapSizeY / 2 - 1) {
                                dy = 1;
                            }
                        }

                        float weightX = Mathf.Abs(x + 0.5f - Constants.chunkSizeX / 2) / (float)Constants.chunkSizeX;
                        float weightY = Mathf.Abs(z + 0.5f - Constants.chunkSizeZ / 2) / (float)Constants.chunkSizeZ;

                        int xCoor = (i + mapSizeX / 2) * Constants.chunkSizeX + x;
                        int yCoor = (j + mapSizeY / 2) * Constants.chunkSizeZ + z;
                        byte topBlock = 0x00, surfaceBlock = 0x00, baseBlock = 0x00;
                        switch (blocks[xCoor, yCoor]) {
                            case 1:
                                topBlock = 0x0C;
                                surfaceBlock = 0x03;
                                baseBlock = 0x01;
                                break;
                            case 2:
                                topBlock = 0x0C;
                                surfaceBlock = 0x18;
                                baseBlock = 0x18;
                                break;
                            case 3:
                                topBlock = 0x02;
                                surfaceBlock = 0x03;
                                baseBlock = 0x01;
                                break;
                            case 4:
                                topBlock = 0x02;
                                surfaceBlock = 0x03;
                                baseBlock = 0x01;
                                break;
                            case 5:
                                topBlock = 0x0C;
                                surfaceBlock = 0x18;
                                baseBlock = 0x18;
                                break;
                            case 6:
                                topBlock = 0x0C;
                                surfaceBlock = 0x18;
                                baseBlock = 0x18;
                                break;
                            case 7:
                                topBlock = 0x0C;
                                surfaceBlock = 0x03;
                                baseBlock = 0x01;
                                break;
                            case 8:
                                topBlock = 0x02;
                                surfaceBlock = 0x03;
                                baseBlock = 0x01;
                                break;
                            case 9:
                                topBlock = 0x1A;
                                surfaceBlock = 0x03;
                                baseBlock = 0x01;
                                break;
                        }

                        float height1 = chunkTerrains[i + mapSizeX / 2, j + mapSizeY / 2].getValue(i * Constants.chunkSizeX + x, j * Constants.chunkSizeZ + z);
                        float height2 = chunkTerrains[i + mapSizeX / 2 + dx, j + mapSizeY / 2].getValue(i * Constants.chunkSizeX + x, j * Constants.chunkSizeZ + z);
                        float height3 = chunkTerrains[i + mapSizeX / 2, j + mapSizeY / 2 + dy].getValue(i * Constants.chunkSizeX + x, j * Constants.chunkSizeZ + z);
                        float height4 = chunkTerrains[i + mapSizeX / 2 + dx, j + mapSizeY / 2 + dy].getValue(i * Constants.chunkSizeX + x, j * Constants.chunkSizeZ + z);

                        int height = Mathf.FloorToInt(   (1.0f - weightX) * (1.0f - weightY) * height1
                                                       + weightX          * (1.0f - weightY) * height2
                                                       + (1.0f - weightX) * weightY          * height3
                                                       + weightX          * weightY          * height4);
                        
                        for (int y = 0; y < height; y++) {
                            if (y == height - 1) arr[x, y, z] = topBlock;
                            else if (y > height - 4) arr[x, y, z] = surfaceBlock;
                            else arr[x, y, z] = baseBlock;
                        }

                        int t = blocks[xCoor, yCoor];
                        

                        if((t == 3 || t == 4 || t == 8) && height >= Constants.SeaLevel) {
                            if(Random.Range(0.0f, 1.0f) < grassfill) {
                                arr[x, height, z] = 0x1B;
                            } else if(Random.Range(0.0f, 1.0f) < flowerfill) {
                                arr[x, height, z] = 0x1C;
                            } else if (Random.Range(0.0f, 1.0f) < flowerfill) {
                                arr[x, height, z] = 0x1D;
                            }
                        }

                        if (t == 8 && height >= Constants.SeaLevel) {
                            if (Random.Range(0.0f, 1.0f) < treefill) {
                                PlaceTree(x, height, z, arr);
                            }
                        }

                        if(t == 2 && height >= Constants.SeaLevel) {
                            if(Random.Range(0.0f, 1.0f) < cactusfill) {
                                PlaceCactus(x, height, z, arr);
                            }
                        }

                        // Put Water
                        for (int y = 0; y < Constants.SeaLevel; y++) {
                            if (arr[x, y, z] == 0x00) arr[x, y, z] = 0x09;
                        }

                    }
                }

                data.blockData = Chunk.encodeByteArray(arr);
                SaveChunkData(data);

            }
        }

        for (int i = -mapSizeX / 2; i < mapSizeX / 2; i++) {
            for (int j = -mapSizeY / 2; j < mapSizeY / 2; j++) {
                Destroy(chunkTerrains[i + mapSizeX / 2, j + mapSizeY / 2].gameObject);
            }
        }

    }

    public void GenerateBiome(int mapSizeX, int mapSizeY) {

        terrain = new byte[mapSizeX, mapSizeY];

        // (1) Generate Ocean
        for (int x = 0; x < mapSizeX; x++) {
            for (int y = 0; y < mapSizeY; y++) {
                if (Random.Range(0.0f, 1.0f) < weightOcean * (new Vector2(x - mapSizeX / 2, y - mapSizeY / 2).magnitude / (float)mapSizeX)) {
                    terrain[x, y] = 1;
                }
            }
        }
        for (int i = 0; i < 5; i++) SmoothBiomes();

        // (2) Generate Other Terrains
        byte[] originalTerrains = { 2, 3, 4, 8, 9 };
        for (int x = 0; x < mapSizeX; x++) {
            for (int y = 0; y < mapSizeY; y++) {
                if (terrain[x, y] != 1) { // ignore ocean
                    terrain[x, y] = originalTerrains[ Mathf.FloorToInt(Random.Range(0.000f, 4.999f)) ];
                }
            }
        }
        for (int i = 0; i < 8; i++) SmoothBiomes();

        // (3) Make Beach and Near Ocean
        for (int x = 0; x < mapSizeX; x++) {
            for (int y = 0; y < mapSizeY; y++) {
                if (terrain[x, y] == 1) {
                    int[] dx = { 1, 0, -1, 0 };
                    int[] dy = { 0, 1, 0, -1 };
                    for (int k = 0; k < 4; k++) {
                        if (isValidPos(x + dx[k], y + dy[k], terrain)) {
                            if (terrain[x + dx[k], y + dy[k]] != 1 && terrain[x + dx[k], y + dy[k]] != 5) {
                                terrain[x, y] = 5;
                                break;
                            }
                        }
                    }
                }
            }
        }
        byte[,] temp = new byte[mapSizeX, mapSizeY];
        for (int x = 0; x < mapSizeX; x++) {
            for (int y = 0; y < mapSizeY; y++) {
                temp[x, y] = terrain[x, y];
                if (terrain[x, y] == 1) {
                    int[] dx = { 1, 0, -1, 0 };
                    int[] dy = { 0, 1, 0, -1 };
                    for (int k = 0; k < 4; k++) {
                        if (isValidPos(x + dx[k], y + dy[k], terrain)) {
                            if (terrain[x + dx[k], y + dy[k]] == 5 || terrain[x + dx[k], y + dy[k]] == 6) {
                                temp[x, y] = 6;
                                break;
                            }
                        }
                    }
                }
            }
        }
        terrain = temp;

        // (4) Make River
        int numRiver = Random.Range(4, 8);
        for (int k = 0; k < numRiver; k++) CreateRiver();

    }

    private void PlaceTree(int x, int y, int z, byte[,,] arr) {
        if (2 <= x && x < Constants.chunkSizeX - 2
        && 2 <= z && z < Constants.chunkSizeZ - 2) {
            int h = Random.Range(3, 6);
            for (int i = 0; i < h; i++) arr[x, y + i, z] = 0x11;
            for (int i = (h + 1) / 2; i < 3 * (h / 2) + (h & 1); i++) {
                int w = Mathf.Min(2, Mathf.Abs( (i - 1)/2 - h/2 ));
                for(int dx = -w; dx <= w; dx++) {
                    for(int dy = -w; dy <= w; dy++) {
                        if (dx == 0 && dy == 0 && i < h) continue;
                        arr[x + dx, y + i, z + dy] = 0x12;
                    }
                }
            }
        }
    }

    private void PlaceCactus(int x, int y, int z, byte[,,] arr) {
        int h = Random.Range(2, 5);
        for (int i = 0; i < h; i++) arr[x, y + i, z] = 0x19;
    }

    // This method is only used to initialize terrain
    private void setBlockInChunk(int x, int y, int z, byte block, Chunk chunk) {
        if (0 <= x && x < Constants.chunkSizeX
        && 0 <= y && y < Constants.chunkSizeY
        && 0 <= z && z < Constants.chunkSizeZ) {
            chunk.blocks[x, y, z] = block;
        }
    }

    private bool isValidPos(int x, int y, byte[,] arr) {
        return 0 <= x && x < arr.GetLength(0) && 0 <= y && y < arr.GetLength(1);
    }

    private void SmoothBiomes() {

        byte[,] temp = new byte[terrain.GetLength(0), terrain.GetLength(1)];
        int[] dx = { 1, 1, 1, 0, 0, -1, -1, -1 };
        int[] dy = { 1, 0, -1, 1, -1, 1, 0, -1 };
        for(int x = 0; x < terrain.GetLength(0); x++) {
            for(int y = 0; y < terrain.GetLength(1); y++) {

                int[] adjacentCount = new int[biomeMax];
                for(int k = 0; k < 8; k++) {
                    if(isValidPos(x + dx[k], y + dy[k], terrain)) {
                        adjacentCount[terrain[x + dx[k], y + dy[k]]]++;
                    }
                }

                int maxE = 0;
                for(int i = 0; i < biomeMax; i++) {
                    if (adjacentCount[i] > adjacentCount[maxE]) maxE = i;
                    else if (adjacentCount[i] == adjacentCount[maxE] && Random.Range(0.0f, 1.0f) > 0.5f) maxE = i;
                }

                temp[x, y] = (byte) maxE;

            }
        }

        terrain = temp;

    }

    private void SmoothTerrains() {

        byte[,] temp = new byte[blocks.GetLength(0), blocks.GetLength(1)];
        int[] dx = { 1, 1, 1, 0, 0, -1, -1, -1 };
        int[] dy = { 1, 0, -1, 1, -1, 1, 0, -1 };
        for (int x = 0; x < blocks.GetLength(0); x++) {
            for (int y = 0; y < blocks.GetLength(1); y++) {

                int[] adjacentCount = new int[128];
                for (int k = 0; k < 8; k++) {
                    if (isValidPos(x + dx[k], y + dy[k], blocks)) {
                        adjacentCount[blocks[x + dx[k], y + dy[k]]]++;
                    }
                }

                int maxE = 0;
                for (int i = 0; i < biomeMax; i++) {
                    if (adjacentCount[i] > adjacentCount[maxE]) maxE = i;
                }

                temp[x, y] = (byte) maxE;

            }
        }

        terrain = temp;

    }

    private void CreateRiver() {

        int x, y;
        while(true) {
            x = Random.Range(0, terrain.GetLength(0));
            y = Random.Range(0, terrain.GetLength(1));
            if (terrain[x, y] == 5) break;
        }

        int length = Random.Range(4, 64);
        int[] dx = { 1, 0, -1, 0 };
        int[] dy = { 0, 1, 0, -1 };
        for(int i = 0; i < length; i++) {

            terrain[x, y] = 7;

            int[] lengthToOcean = new int[4];
            for(int k = 0; k < 4; k++) {
                int curX = x, curY = y;
                while(isValidPos(curX, curY, terrain) && terrain[curX, curY] != 1) {
                    curX += dx[k];
                    curY += dy[k];
                    lengthToOcean[k]++;
                }
                if (!isValidPos(curX, curY, terrain)) lengthToOcean[k] = 0;
            }

            int[] sum = new int[4];
            for(int k = 0; k < 4; k++) {
                if (k == 0) sum[k] = lengthToOcean[k];
                else sum[k] = sum[k - 1] + lengthToOcean[k];
            }

            int idx = Random.Range(0, sum[3]);
            for(int k = 0; k < 4; k++) {
                if(idx < sum[k]) {
                    x += dx[k]; y += dy[k]; break;
                }
            }

        }

    }

    public void SaveChunkData(ChunkData data) {

        BinaryFormatter binaryFormatter = new BinaryFormatter();

        if (!Directory.Exists(mapPath + "/chunks")) {
            Directory.CreateDirectory(mapPath + "/chunks");
        }

        FileStream file;
        string path = mapPath + "/chunks/" + data.id + ".dat";

        file = File.Create(path);

        binaryFormatter.Serialize(file, data);
        file.Close();

    }

}
