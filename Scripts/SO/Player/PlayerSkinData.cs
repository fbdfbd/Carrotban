using UnityEngine;

[CreateAssetMenu(menuName = "Player/PlayerSkinData")]
public class PlayerSkinData : ScriptableObject
{
    public string skinName;
    public Sprite[] moveSprites;  
    public Sprite[] hoeSprites;  
    public Sprite[] axeSprites; 
    public Sprite[] wateringSprites; 

    public Sprite[] GetSpritesByDirection(Vector2 direction)
    {
        if (direction == Vector2.down) return moveSprites[0..4]; 
        if (direction == Vector2.up) return moveSprites[4..8]; 
        if (direction == Vector2.left) return moveSprites[8..12];
        if (direction == Vector2.right) return moveSprites[12..16]; 
        return moveSprites[0..4];
    }

    public Sprite[] GetSpritesByAction(ActionType actionType, Vector2 direction)
    {
        int offset = direction == Vector2.down ? 0 :
                     direction == Vector2.up ? 2 :
                     direction == Vector2.left ? 4 :
                     direction == Vector2.right ? 6 : 0;

        switch (actionType)
        {
            case ActionType.Hoe: return hoeSprites[offset..(offset + 2)];
            case ActionType.Axe: return axeSprites[offset..(offset + 2)];
            case ActionType.Watering: return wateringSprites[offset..(offset + 2)];
            default: return null;
        }
    }
}
