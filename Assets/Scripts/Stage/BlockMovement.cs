using UnityEngine;
using UnityEngine.Tilemaps;

public class BlockMovement : MonoBehaviour
{
    private Vector3Int currentTilePos;
    [SerializeField] private float baseMoveDuration = 0.3f;
    public float MoveDelay => baseMoveDuration;

    private bool isMoving = false;
    public bool IsMoving => isMoving;

    void Start()
    {
        Tilemap walkableTilemap = GridManager.Instance?.walkableTilemap;
        if (walkableTilemap != null)
        {
            currentTilePos = walkableTilemap.WorldToCell(transform.position);
            transform.position = walkableTilemap.GetCellCenterWorld(currentTilePos);


            GridManager.Instance?.OccupyTile(currentTilePos, gameObject);
            Debug.Log($"[Block] {gameObject.name} started at {currentTilePos} and occupied.");
        }
        else
        {
            Debug.LogError($"[Block] GridManager or Walkable Tilemap not found for {gameObject.name}!");
        }
    }

    public bool CanBePushed(Vector3Int direction)
    {
        if (isMoving)
        {
            return false;
        }

        GridManager gridManager = GridManager.Instance;
        if (gridManager == null) return false;

        Vector3Int targetTilePos = currentTilePos + direction; 

        GameObject occupant = gridManager.GetOccupant(targetTilePos);
        bool hasTile = gridManager.walkableTilemap != null && gridManager.walkableTilemap.HasTile(targetTilePos);

        bool canPush = (occupant == null && hasTile);

        return canPush;
    }

    public void SetCurrentTilePos(Vector3Int pos)
    {
        currentTilePos = pos;
    }

    public Vector3Int GetCurrentTilePos()
    {
        return currentTilePos;
    }

    public void SetMovingState(bool moving)
    {
        isMoving = moving;
    }
}