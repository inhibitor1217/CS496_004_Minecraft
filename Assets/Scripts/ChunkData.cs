using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChunkData {

    public string id;
    public int chunkX, chunkY;
    public byte[,,] blockData;
    // public SerializableVector3[] verticesData;
    // public int[] trianglesData;
    // public SerializableVector2[] uvData;

}

/*
 * 
[Serializable]
public struct SerializableVector3 {

    public float x, y, z;

    public SerializableVector3(float _x, float _y, float _z) {
        x = _x;
        y = _y;
        z = _z;
    }

    public override string ToString() {
        return String.Format("[{0}, {1}, {2}]", x, y, z);
    }

    public static implicit operator Vector3(SerializableVector3 v) {
        return new Vector3(v.x, v.y, v.z);
    }

    public static implicit operator SerializableVector3(Vector3 v) {
        return new SerializableVector3(v.x, v.y, v.z);
    }

}

[Serializable]
public struct SerializableVector2 {

    public float x, y;

    public SerializableVector2(float _x, float _y) {
        x = _x;
        y = _y;
    }

    public override string ToString() {
        return String.Format("[{0}, {1}]", x, y);
    }

    public static implicit operator Vector2(SerializableVector2 v) {
        return new Vector3(v.x, v.y);
    }

    public static implicit operator SerializableVector2(Vector2 v) {
        return new SerializableVector2(v.x, v.y);
    }

}

public static class ArrayCast {

    public static Vector3[] Cast(SerializableVector3[] arr) {
        Vector3[] ret = new Vector3[arr.Length];
        for(int i = 0; i < arr.Length; i++) {
            ret[i] = arr[i];
        }
        return ret;
    }

    public static SerializableVector3[] Cast(Vector3[] arr) {
        SerializableVector3[] ret = new SerializableVector3[arr.Length];
        for (int i = 0; i < arr.Length; i++) {
            ret[i] = arr[i];
        }
        return ret;
    }

    public static Vector2[] Cast(SerializableVector2[] arr) {
        Vector2[] ret = new Vector2[arr.Length];
        for (int i = 0; i < arr.Length; i++) {
            ret[i] = arr[i];
        }
        return ret;
    }

    public static SerializableVector2[] Cast(Vector2[] arr) {
        SerializableVector2[] ret = new SerializableVector2[arr.Length];
        for (int i = 0; i < arr.Length; i++) {
            ret[i] = arr[i];
        }
        return ret;
    }

}

*/