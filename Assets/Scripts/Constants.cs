using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants {

    // Chunk Size
    public static int chunkSizeX = 16, chunkSizeY = 128, chunkSizeZ = 16;
    public static int chunkLogSizeX = 4, chunkLogSizeY = 7, chunkLogSizeZ = 4;
    public static byte chunkMaskX = 0x0F, chunkMaskY = 0x7F, chunkMaskZ = 0x0F;

    // Visible Chunk Distance
    public static int visibleChunkDistance = 3;
    public static int chunkArraySize = 8;
    public static int chunkArrayLogSize = 3;
    public static int chunkArrayMask = 0x07;

    // Texture Table                      TOP   RIG   LEF   FRO   BAC   BOT
    public static byte[,] Resource = { { 0xB4, 0xB4, 0xB4, 0xB4, 0xB4, 0xB4 },   // 0 Air
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

    // Block Hardness Table               0      1      2      3      4      5      6      7      8      9
    public static float[] Hardness = {
                                       0.1f,  7.5f,  0.9f,  0.9f, 10.0f,  5.0f, -1.0f, -1.0f, -1.0f, -1.0f,  // 0
                                      -1.0f, -1.0f, 0.75f,  0.9f, 15.0f, 15.0f, 15.0f,  3.0f                 // 1 
                                     };

    // Transparent Blocks
    public static byte[] TransparentBlocks = { 0x00, 0x06 };

    public static bool isTransparent(byte b) {
        bool ret = false;
        for (int i = 0; i < Constants.TransparentBlocks.Length; i++) {
            if (b == TransparentBlocks[i]) { ret = true; break; }
        }
        return ret;
    }

}
