using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    private Vector3Int currentTilePos;
    [SerializeField] private float moveDelay = 0.4f;
    public float MoveDelay => moveDelay;

    private bool isMoving = false;
    public bool IsMoving => isMoving;

    private void Start()
    {
        Tilemap walkableTilemap = GridManager.Instance?.walkableTilemap;
        if (walkableTilemap != null)
        {
            currentTilePos = walkableTilemap.WorldToCell(transform.position);

            transform.position = walkableTilemap.GetCellCenterWorld(currentTilePos);

            GridManager.Instance.OccupyTile(currentTilePos, gameObject);
        }
        else
        {
            Debug.LogError("타일맵이없습니다");
            currentTilePos = Vector3Int.zero; // 임시 처리
        }
    }

    public void SetMoveSpeed(float multiplier)
    {
        moveDelay = 0.4f / multiplier;
    }

    public bool CanMove(Vector3Int direction, out Vector3Int targetTilePos, out bool pushesBlock, out GameObject blockToPush)
    {
        targetTilePos = currentTilePos + direction;
        pushesBlock = false;
        blockToPush = null;

        if (isMoving)
        {
            return false;
        }

        GridManager gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            Debug.LogError("[Player.CanMove] GridManager is null!");
            return false;
        }

        GameObject occupant = gridManager.GetOccupant(targetTilePos);

        if (occupant == null && gridManager.walkableTilemap != null && gridManager.walkableTilemap.HasTile(targetTilePos))
        {
            return true;
        }

        else if (occupant != null)
        {

            BlockMovement blockMovement = occupant.GetComponent<BlockMovement>();
            if (blockMovement != null)
            {
                if (blockMovement.CanBePushed(direction))
                {
                    pushesBlock = true;
                    blockToPush = occupant;
                    return true; 
                }
                else
                {
                    Debug.Log($"Block {occupant.name} 안밀림");
                }
            }
            else
            {
                Debug.Log($"Block 이 아님");
            }
        }

        return false;
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