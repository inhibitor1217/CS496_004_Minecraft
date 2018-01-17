using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants {

    // Map Size
    public static int mapSizeX = 64, mapSizeY = 64;

    // Chunk Size
    public static int chunkSizeX = 16, chunkSizeY = 128, chunkSizeZ = 16;
    public static int chunkLogSizeX = 4, chunkLogSizeY = 7, chunkLogSizeZ = 4;
    public static byte chunkMaskX = 0x0F, chunkMaskY = 0x7F, chunkMaskZ = 0x0F;

    // Visible Chunk Distance
    public static int visibleChunkDistance = 5;
    public static int chunkArraySize = 16;
    public static int chunkArrayLogSize = 4;
    public static int chunkArrayMask = 0x0F;

    // Sea Level
    public static int SeaLevel = 63;

    // Terrain Indices
    public static byte Empty = 0;
    public static byte Ocean = 1;
    public static byte Desert = 2;
    public static byte Hills = 3;
    public static byte Plains = 4;
    public static byte Beach = 5;
    public static byte NearOcean = 6;
    public static byte River = 7;
    public static byte Forest = 8;
    public static byte SnowHills = 9;

    // Texture Table                     TOP   RIGHT   LEFT  FRONT   BACK   BOT
    public static int[,] Resource = { { 0x0B4, 0x0B4, 0x0B4, 0x0B4, 0x0B4, 0x0B4 },   // 00 Air
                                      { 0x000, 0x000, 0x000, 0x000, 0x000, 0x000 },   // 01 Stone
                                      { 0x100, 0x003, 0x003, 0x003, 0x003, 0x002 },   // 02 Grass
                                      { 0x002, 0x002, 0x002, 0x002, 0x002, 0x002 },   // 03 Dirt
                                      { 0x010, 0x010, 0x010, 0x010, 0x010, 0x010 },   // 04 Cobblestone
                                      { 0x004, 0x004, 0x004, 0x004, 0x004, 0x004 },   // 05 Wood Plank
                                      { 0x0B4, 0x0B4, 0x0B4, 0x0B4, 0x0B4, 0x0B4 },   // 06 Sapling
                                      { 0x011, 0x011, 0x011, 0x011, 0x011, 0x011 },   // 07 Bedrock
                                      { 0x0CF, 0x0CF, 0x0CF, 0x0CF, 0x0CF, 0x0CF },   // 08 Flowing Water
                                      { 0x0CF, 0x0CD, 0x0CD, 0x0CD, 0x0CD, 0x0CF },   // 09 Still Water
                                      { 0x0FF, 0x0FF, 0x0FF, 0x0FF, 0x0FF, 0x0FF },   // 0A Flowing Lava
                                      { 0x0FF, 0x0FF, 0x0FF, 0x0FF, 0x0FF, 0x0FF },   // 0B Still Lava
                                      { 0x012, 0x012, 0x012, 0x012, 0x012, 0x012 },   // 0C Sand
                                      { 0x013, 0x013, 0x013, 0x013, 0x013, 0x013 },   // 0D Gravel
                                      { 0x020, 0x020, 0x020, 0x020, 0x020, 0x020 },   // 0E Gold Ore
                                      { 0x021, 0x021, 0x021, 0x021, 0x021, 0x021 },   // 0F Iron Ore
                                      { 0x022, 0x022, 0x022, 0x022, 0x022, 0x022 },   // 10 Coal Ore
                                      { 0x015, 0x014, 0x014, 0x014, 0x014, 0x015 },   // 11 Oak Wood
                                      { 0x102, 0x102, 0x102, 0x102, 0x102, 0x102 },   // 12 Oak Leaves
                                      { 0x030, 0x030, 0x030, 0x030, 0x030, 0x030 },   // 13 Sponge
                                      { 0x031, 0x031, 0x031, 0x031, 0x031, 0x031 },   // 14 Glass
                                      { 0x0A0, 0x0A0, 0x0A0, 0x0A0, 0x0A0, 0x0A0 },   // 15 Lapis Lazuli Ore
                                      { 0x090, 0x090, 0x090, 0x090, 0x090, 0x090 },   // 16 Lapis Lazuli Block
                                      { 0x03E, 0x02D, 0x02D, 0x02E, 0x02D, 0x03E },   // 17 Dispenser
                                      { 0x0B0, 0x0C0, 0x0C0, 0x0D0, 0x0D0, 0x0B0 },   // 18 Sandstone
                                      { 0x045, 0x046, 0x046, 0x046, 0x046, 0x045 },   // 19 Cactus
                                      { 0x042, 0x044, 0x044, 0x044, 0x044, 0x002 },   // 1A Snow
                                      { 0x027, 0x000, 0x000, 0x000, 0x000, 0x000 },   // 1B Grass
                                      { 0x00C, 0x000, 0x000, 0x000, 0x000, 0x000 },   // 1C Poppy
                                      { 0x00D, 0x000, 0x000, 0x000, 0x000, 0x000 },   // 1D Dandelion
                                      { 0x039, 0x000, 0x000, 0x000, 0x000, 0x000 },   // 1E Sugar Cane
                                    };

    // Block Hardness Table               0      1      2      3      4      5      6      7      8      9
    public static float[] Hardness = {
                                       0.1f,  7.5f,  0.9f,  0.9f, 10.0f,  5.0f, -1.0f, -1.0f, -1.0f, -1.0f,  // 0
                                      -1.0f, -1.0f, 0.75f,  0.9f, 15.0f, 15.0f, 15.0f,  3.0f, 0.35f,  0.9f,  // 1
                                      0.45f, 15.0f, 15.0f, 17.5f,  4.0f, 0.65f,  0.9f, 0.01f, 0.01f, 0.01f,  // 2
                                      0.01f                                                                  // 3
                                     };

    // Transparent Blocks
    public static byte[] TransparentBlocks = { 0x00, 0x06, 0x1B, 0x1C, 0x1D };

    // Blocks that Require Special Rendering
    public static byte[] SmallBlocks = { 0x19 };
    public static byte[] DiagonalBlocks = { 0x1B, 0x1C, 0x1D, 0x1E };

    // Blocks that are not solid
    public static byte[] NonSolidBlocks = { 0x00, 0x06, 0x1B, 0x1C, 0x1D, 0x1E };

    public static bool isTransparent(byte b) {
        bool ret = false;
        for (int i = 0; i < Constants.TransparentBlocks.Length; i++) {
            if (b == TransparentBlocks[i]) { ret = true; break; }
        }
        return ret;
    }
    
    public static bool isSmall(byte b) {
        bool ret = false;
        for (int i = 0; i < Constants.SmallBlocks.Length; i++) {
            if (b == SmallBlocks[i]) { ret = true; break; }
        }
        return ret;
    }

    public static bool isDiagonal(byte b) {
        bool ret = false;
        for (int i = 0; i < Constants.DiagonalBlocks.Length; i++) {
            if (b == DiagonalBlocks[i]) { ret = true; break; }
        }
        return ret;
    }

    public static bool isNonSolid(byte b) {
        bool ret = false;
        for (int i = 0; i < Constants.NonSolidBlocks.Length; i++) {
            if (b == NonSolidBlocks[i]) { ret = true; break; }
        }
        return ret;
    }

}
