using UnityEngine;

public class Block : MonoBehaviour
{
    private BlockMovement blockMovement;

    private void Awake()
    {
        blockMovement = GetComponent<BlockMovement>();
        if (blockMovement == null)
        {
            Debug.LogError($"Block {gameObject.name} is missing BlockMovement component!");
        }
    }

    public BlockMovement GetMovementComponent() => blockMovement;
}