using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public Tilemap walkableTilemap;

    private Dictionary<Vector3Int, GameObject> occupiedTiles = new Dictionary<Vector3Int, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void SetWalkableTilemap(Tilemap tilemap)
    {
        walkableTilemap = tilemap;
    }

    public bool IsTileWalkable(Vector3Int tilePos)
    {
        if (walkableTilemap == null)
        {
            Debug.LogError("walkableTilemap null");
            return false;
        }

        bool exists = walkableTilemap.HasTile(tilePos);

        bool occupied = GetOccupant(tilePos) != null;
        return exists && !occupied;
    }

    public void OccupyTile(Vector3Int tilePos, GameObject obj)
    {
        if (!occupiedTiles.ContainsKey(tilePos))
        {
            occupiedTiles[tilePos] = obj;
        }
        else
        {
            if (occupiedTiles[tilePos] != obj)
            {
                Debug.LogWarning($"타일 {tilePos} 이미 {occupiedTiles[tilePos].name} 점유 {obj.name} 불가");
            }
        }
    }

    public void VacateTile(Vector3Int tilePos)
    {
        if (occupiedTiles.ContainsKey(tilePos))
        {
            occupiedTiles.Remove(tilePos);
        }
    }

    public GameObject GetOccupant(Vector3Int tilePos)
    {
        occupiedTiles.TryGetValue(tilePos, out GameObject occupant);
        return occupant;
    }

    public void ClearOccupancy()
    {
        occupiedTiles.Clear();
    }
}