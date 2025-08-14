using UnityEngine;
using System.Collections;

public class PlayerAnimation : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public PlayerSkinData currentSkin;

    private Sprite[] currentDirectionSprites;
    private Vector2 currentDirection = Vector2.down;
    private bool isPerformingAction = false;
    private float moveDelay = 0.4f; //이게기본
    private float moveAnimationSpeed;

    public void SetAnimationSpeed(float multiplier)
    {
        moveAnimationSpeed = moveDelay / multiplier;
    }

    public void PlayMoveAnimation(Vector2 direction, bool canMove)
    {
        if (isPerformingAction) return;

        currentDirection = direction;
        currentDirectionSprites = currentSkin.GetSpritesByDirection(direction);

        StopAllCoroutines();

        if (canMove)
        {
            StartCoroutine(PlayMoveCycle());
        }
        else
        {
            ResetToIdleSprite();
        }
    }

    private IEnumerator PlayMoveCycle()
    {
        int totalFrames = currentDirectionSprites.Length;
        float frameTime = moveAnimationSpeed / totalFrames;

        for (int i = 0; i < totalFrames; i++)
        {
            spriteRenderer.sprite = currentDirectionSprites[i];
            yield return new WaitForSeconds(frameTime);
        }
        ResetToIdleSprite();
    }

    public void StopSwappingSprite()
    {
        StopAllCoroutines();
        ResetToIdleSprite();
    }

    public void ResetToIdleSprite()
    {
        if (currentDirectionSprites == null || currentDirectionSprites.Length == 0) return;
        spriteRenderer.sprite = currentDirectionSprites[0];
    }

    public void PlayAction(ActionType actionType)
    {
        if (isPerformingAction) return;

        Sprite[] actionSprites = currentSkin.GetSpritesByAction(actionType, currentDirection);
        if (actionSprites != null && actionSprites.Length > 0)
        {
            isPerformingAction = true;
            StopAllCoroutines();
            StartCoroutine(PlayActionAnimation(actionSprites));
        }
    }

    private IEnumerator PlayActionAnimation(Sprite[] actionSprites)
    {
        float frameTime = moveAnimationSpeed / actionSprites.Length;
        for (int i = 0; i < actionSprites.Length; i++)
        {
            spriteRenderer.sprite = actionSprites[i];
            yield return new WaitForSeconds(frameTime);
        }
        isPerformingAction = false;
        ResetToIdleSprite();
    }

    public bool IsPerformingAction()
    {
        return isPerformingAction;
    }
}
