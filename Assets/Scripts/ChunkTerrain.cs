using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkTerrain : MonoBehaviour {

    public AnimationCurve noiseCurve;
    public float offsetX, offsetY;

    public float amplitude = 1.0f;
    public float altitude = 0.0f;

    public float getValue(float x, float y) {

        float ret = 0.0f;

        float maxV = 0.0001f;
        for(int i = 0; i < 16; i++) {
            float curV = noiseCurve.Evaluate((int)i / 16.0f);
            if (maxV < curV) maxV = curV;
        }

        for(int i = 0; i <= 16; i++) {
            float f = (float)i / 16.0f;
            ret += amplitude * noiseCurve.Evaluate(f) / maxV * (Mathf.PerlinNoise(offsetX + x * f * 0.1f, offsetY + y * f * 0.1f) - 0.5f);
        }

        return 60.0f + altitude + ret;

    }

}
