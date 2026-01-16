using UnityEngine;
using UnityEngine.Tilemaps; 

[CreateAssetMenu(menuName = "Map/TileData")]
public class TileData : ScriptableObject
{
    public TileType tileType;
    public TileBase tileBase;
    public bool isWalkable;

    public TileType changesTo;
}