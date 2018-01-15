using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MapSelectionButton : MonoBehaviour {

    public string path;
    
    public void DeleteMap() {

        if(Directory.Exists(path)) {
            Directory.Delete(path, true);
            GameObject.FindGameObjectWithTag("MapSelectionUI").GetComponent<MapSelectionUI>().Reload();
        }

    }

    public void LoadMap() {

        MainSceneLoadInfo.info.mapPath = path;
        MainSceneLoadInfo.info.shouldGenerateMap = false;
        Application.LoadLevel(1);

    }

    public void CreateUI() {

        Application.LoadLevel(2);

    }

    public void Home() {

        Application.LoadLevel(0);

    }

    public void CreateMap() {

        MainSceneLoadInfo.info.mapPath = path;
        MainSceneLoadInfo.info.shouldGenerateMap = true;
        Application.LoadLevel(1);
    }

}
