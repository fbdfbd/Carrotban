using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    private PlayerAnimation playerAnimation;
    private PlayerMovement playerMovement;
    private Vector2 lastDirection = Vector2.down;
    public Vector2 LastDirection => lastDirection;

    void Start()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void MoveUp() { lastDirection = Vector2.up; TryStartMove(Vector3Int.up); }
    public void MoveDown() { lastDirection = Vector2.down; TryStartMove(Vector3Int.down); }
    public void MoveLeft() { lastDirection = Vector2.left; TryStartMove(Vector3Int.left); }
    public void MoveRight() { lastDirection = Vector2.right; TryStartMove(Vector3Int.right); }

    private void TryStartMove(Vector3Int direction)
    {
        Vector3Int playerTargetTile; 
        bool pushesBlock;            
        GameObject blockToPush;    
 
        bool canMove = playerMovement.CanMove(direction, out playerTargetTile, out pushesBlock, out blockToPush);

        playerAnimation?.PlayMoveAnimation(lastDirection, canMove);

        if (canMove)
        {
            Vector3Int playerCurrentTile = playerMovement.GetCurrentTilePos();
            float playerMoveDuration = playerMovement.MoveDelay;

            List<Command> commandsToExecute = new List<Command>();

            if (pushesBlock && blockToPush != null)
            {
                BlockMovement bm = blockToPush.GetComponent<BlockMovement>();
                if (bm != null)
                {
                    Vector3Int blockCurrentTile = playerTargetTile;
                    Vector3Int blockTargetTile = blockCurrentTile + direction;
                    float blockMoveDuration = bm.MoveDelay;

                    var blockMoveCommand = new MoveCommand(blockToPush, blockCurrentTile, blockTargetTile, blockMoveDuration);
                    commandsToExecute.Add(blockMoveCommand);
                }
                else { Debug.LogError($"Block {blockToPush.name} is missing BlockMovement script!"); return; }
            }

            var playerMoveCommand = new MoveCommand(gameObject, playerCurrentTile, playerTargetTile, playerMoveDuration);
            commandsToExecute.Add(playerMoveCommand);

            playerMovement.SetMovingState(true);

            foreach (var cmd in commandsToExecute)
            {
                cmd.Execute();
            }

            if (commandsToExecute.Count > 1)
            {
                var composite = new CompositeCommand(commandsToExecute);
                UndoManager.Instance.RecordCommand(composite);
            }
            else if (commandsToExecute.Count == 1)
            {
                UndoManager.Instance.RecordCommand(commandsToExecute[0]);
            }
        }
    }

    public void PerformAction()
    {
        if (playerMovement.IsMoving || (playerAnimation != null && playerAnimation.IsPerformingAction()))
        {
            return;
        }

        Vector3Int playerCurrentTile = playerMovement.GetCurrentTilePos();
        Vector3Int targetTile = playerCurrentTile + GetVector3IntFromDirection(lastDirection);

        GridManager gridManager = GridManager.Instance;
        if (gridManager == null) { Debug.LogError("[Controller.PerformAction] GridManager is null!"); return; }

        GameObject occupant = gridManager.GetOccupant(targetTile);

        if (occupant != null)
        {
            Player playerComponent = GetComponent<Player>(); 
            if (playerComponent == null) { Debug.LogError("Player component missing!"); return; }

            //³ª¹«
            InteractableTree tree = occupant.GetComponent<InteractableTree>();
            if (tree != null)
            {
                var chopCommand = new ChopTreeCommand(playerComponent, tree);
                playerAnimation?.PlayAction(ActionType.Axe); 
                chopCommand.Execute();                      
                UndoManager.Instance.RecordCommand(chopCommand);
                return; // ¾×¼Ç ¼öÇà ¿Ï·á
            }

            //¹ç
            Farmland farmland = occupant.GetComponent<Farmland>();
            if (farmland != null)
            {
                Command actionCommand = null;

                switch (farmland.CurrentState)
                {
                    case Farmland.FarmlandState.Normal: 
                        actionCommand = new HoeCommand(playerComponent, farmland);
                        playerAnimation?.PlayAction(ActionType.Hoe); 
                        break;
                    case Farmland.FarmlandState.Plowed: 
                        actionCommand = new WaterCommand(playerComponent, farmland);
                        playerAnimation?.PlayAction(ActionType.Watering);
                        break;
                    case Farmland.FarmlandState.Watered:
                        Debug.Log("¹°ÁØ¶¥");
                        break;
                }

                if (actionCommand != null)
                {
                    actionCommand.Execute();
                    UndoManager.Instance.RecordCommand(actionCommand);
                }
                return; 
            }
        }
        else
        {
            Debug.Log("ºó ¶¥");
        }
    }

    private Vector3Int GetVector3IntFromDirection(Vector2 dir)
    {
        if (dir == Vector2.up) return Vector3Int.up;
        if (dir == Vector2.down) return Vector3Int.down;
        if (dir == Vector2.left) return Vector3Int.left;
        if (dir == Vector2.right) return Vector3Int.right;
        return Vector3Int.zero;
    }
}