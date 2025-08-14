using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Map/MapData")]
public class MapData : ScriptableObject
{
    public int mapName;
    public int width;
    public int height;
    public List<TileRow> tiles;
    public Vector2Int playerStartPos;
    public string tileSetFolder;
}

[System.Serializable]
public class TileRow
{
    public List<int> row;
}
