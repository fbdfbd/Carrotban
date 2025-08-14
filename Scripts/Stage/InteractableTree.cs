using UnityEngine;
using DG.Tweening;
using UnityEngine.Tilemaps;

public class InteractableTree : MonoBehaviour
{
    public enum TreeState { Healthy, Damaged, Fallen }

    [Header("스프라이트 설정")]
    [SerializeField] private Sprite treeDefault;
    [SerializeField] private Sprite treeFallingSprite;
    [SerializeField] private Sprite treeStumpSprite;

    [Header("옵션: DOTween 흔들림 효과")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeStrength = 0.1f;

    private SpriteRenderer spriteRenderer;
    private int hitCount = 0;
    private const int maxHitCount = 4;
    private TreeState currentState = TreeState.Healthy;
    private Vector3Int currentTilePos;


    public int HitCount => hitCount;   
    public TreeState CurrentState => currentState;
    public Vector3Int CurrentTilePos => currentTilePos;
    void Start()
    {
        TreeInit();

        Tilemap walkableTilemap = GridManager.Instance?.walkableTilemap;
        if (walkableTilemap != null && GridManager.Instance != null) 
        {
            Vector3 currentWorldPos = transform.position; 
            currentTilePos = walkableTilemap.WorldToCell(currentWorldPos);
            GridManager.Instance.OccupyTile(currentTilePos, gameObject);
        }
        else Debug.LogError($"[InteractableTree Start] : 못찾음 {gameObject.name}");
    }

    public void TreeInit()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentState = TreeState.Healthy;
        hitCount = 0;
        if (spriteRenderer != null && treeDefault != null)
            spriteRenderer.sprite = treeDefault;
    }

    public void Hit()
    {
        if (currentState == TreeState.Fallen) return;

        hitCount++;

        if (hitCount < maxHitCount)
        {
            currentState = TreeState.Damaged; 
            PlayDamageAnimation();
        }
        else
        {
            PlayFallingAnimation();
        }
    }

    private void PlayDamageAnimation()
    {
        transform.DOShakePosition(shakeDuration, shakeStrength, 10, 90, false, true);
    }

    private void PlayFallingAnimation()
    {
        if (currentState == TreeState.Fallen) return;

        currentState = TreeState.Fallen;

        transform.DOKill();

        Sequence fallSequence = DOTween.Sequence();
        if (treeFallingSprite != null)
        {
            fallSequence.AppendCallback(() => spriteRenderer.sprite = treeFallingSprite);
            fallSequence.AppendInterval(0.5f);
        }
        fallSequence.AppendCallback(() =>
        {
            if (treeStumpSprite != null)
                spriteRenderer.sprite = treeStumpSprite;
        });
    }

    public void RestoreState(TreeState stateToRestore, int hitCountToRestore)
    {
        StopAllCoroutines(); 
        transform.DOKill();

        currentState = stateToRestore;
        hitCount = hitCountToRestore;

        if (spriteRenderer != null)
        {
            if (currentState == TreeState.Healthy || currentState == TreeState.Damaged)
            {
                spriteRenderer.sprite = treeDefault;
            }
            else 
            {
                spriteRenderer.sprite = treeStumpSprite;
            }
        }
    }
}