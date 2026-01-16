using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GameMaps
{
    public MapJsonData[] maps;
}

[System.Serializable]
public class MapJsonData
{
    public int name;
    public int width;
    public int height;
    public List<List<int>> tiles;
    public Vector2Int playerStart;
    public string tileSetFolder;
}


