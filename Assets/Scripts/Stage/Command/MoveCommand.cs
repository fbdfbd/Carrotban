using UnityEngine;
using DG.Tweening;

public class MoveCommand : Command
{
    private GameObject targetObject;
    private Vector3Int fromTile;
    private Vector3Int toTile;
    private float animationDuration;

    private GridManager gridManager;
    private PlayerMovement playerMovement;
    private BlockMovement blockMovement;
    private PlayerAnimation playerAnimation;

    public MoveCommand(GameObject target, Vector3Int from, Vector3Int to, float duration)
    {
        targetObject = target;
        fromTile = from;
        toTile = to;
        animationDuration = duration;

        gridManager = GridManager.Instance;
        playerMovement = targetObject.GetComponent<PlayerMovement>();
        blockMovement = targetObject.GetComponent<BlockMovement>();
        playerAnimation = targetObject.GetComponent<PlayerAnimation>();
    }

    public override void Execute()
    {
        if (gridManager != null)
        {
            gridManager.VacateTile(fromTile);
            gridManager.OccupyTile(toTile, targetObject);
        }
        if (playerMovement != null) playerMovement.SetCurrentTilePos(toTile);
        else if (blockMovement != null) blockMovement.SetCurrentTilePos(toTile);

        if (gridManager != null && gridManager.walkableTilemap != null)
        {
            Vector3 targetWorldPos = gridManager.walkableTilemap.GetCellCenterWorld(toTile);
            targetObject.transform.DOMove(targetWorldPos, animationDuration)
                .SetEase(Ease.Linear) 
                .OnComplete(() => {
                    if (playerMovement != null) playerMovement.SetMovingState(false);
                    else if (blockMovement != null) blockMovement.SetMovingState(false);
                });
        }
        else
        {
            Debug.LogWarning($"[MoveCommand.Execute]: {targetObject.name}");
            Vector3 targetWorldPos = new Vector3(toTile.x + 0.5f, toTile.y + 0.5f, targetObject.transform.position.z);
            targetObject.transform.position = targetWorldPos;

            if (playerMovement != null) playerMovement.SetMovingState(false);
            else if (blockMovement != null) blockMovement.SetMovingState(false);
        }
    }

    public override void Undo()
    {
        if (gridManager != null)
        {
            gridManager.VacateTile(toTile);
            gridManager.OccupyTile(fromTile, targetObject);
        }
        if (playerMovement != null) playerMovement.SetCurrentTilePos(fromTile);
        else if (blockMovement != null) blockMovement.SetCurrentTilePos(fromTile);

        if (gridManager != null && gridManager.walkableTilemap != null)
        {
            Vector3 originalWorldPos = gridManager.walkableTilemap.GetCellCenterWorld(fromTile);
            targetObject.transform.DOMove(originalWorldPos, animationDuration)
                .SetEase(Ease.Linear) 
                .OnComplete(() => {
                    if (playerMovement != null) playerMovement.SetMovingState(false);
                    else if (blockMovement != null) blockMovement.SetMovingState(false);
                    playerAnimation?.ResetToIdleSprite();
                });
        }
        else
        {
            Debug.LogWarning($"[MoveCommand.Undo]: {targetObject.name}");
            Vector3 originalWorldPos = new Vector3(fromTile.x + 0.5f, fromTile.y + 0.5f, targetObject.transform.position.z);
            targetObject.transform.position = originalWorldPos;
            if (playerMovement != null) playerMovement.SetMovingState(false);
            else if (blockMovement != null) blockMovement.SetMovingState(false);
            playerAnimation?.ResetToIdleSprite();
        }
        playerAnimation?.ResetToIdleSprite();
    }
}