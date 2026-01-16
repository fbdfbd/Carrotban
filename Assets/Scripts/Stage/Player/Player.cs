using UnityEngine;

public class Player : MonoBehaviour
{
    public string playerName = "Default";
    public Sprite currentSkin;
    public Color skinColor = Color.white;
    public float speedMultiplier = 1f;
    public bool hasSeed = false;
    

    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;
    private PlayerMovement playerMovement;
    private PlayerAnimation playerAnimation;

    public PlayerController PlayerController => playerController;
    public PlayerMovement PlayerMovement => playerMovement;

    public bool HasSeed => hasSeed;

    public event System.Action<bool> OnHasSeedChanged;


    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    public void PlayerInit()
    {
        SetPlayerSpeed(speedMultiplier);
    }

    public void SetPlayerSpeed(float multiplier)
    {
        speedMultiplier = multiplier;
        playerAnimation?.SetAnimationSpeed(multiplier);
        playerMovement?.SetMoveSpeed(multiplier);
    }

    public void SetSkin(Sprite newSkin)
    {
        currentSkin = newSkin;
        ApplySkin();
    }

    private void ApplySkin()
    {
        if (spriteRenderer != null && currentSkin != null)
            spriteRenderer.sprite = currentSkin;
    }

    
    private void OnValidate()
    {
        SetPlayerSpeed(speedMultiplier);
    }

    public void SetSeedPossession(bool possess)
    {
        if (hasSeed != possess)
        {
            hasSeed = possess;
            OnHasSeedChanged?.Invoke(hasSeed);
        }
    }
}
