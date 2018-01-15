using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneLoadInfo : MonoBehaviour {

    public static MainSceneLoadInfo info;

    public string mapPath;
    public bool shouldGenerateMap;

    private void Awake() {
        if(info == null) {
            DontDestroyOnLoad(gameObject);
            info = this;
            if(!Directory.Exists(Application.persistentDataPath + "/maps")) {
                Directory.CreateDirectory(Application.persistentDataPath + "/maps");
            }
        } else if(info != this) {
            Destroy(gameObject);
        }
    }

}
