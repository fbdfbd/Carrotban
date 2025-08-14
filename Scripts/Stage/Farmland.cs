using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using UnityEngine.Events;

public class Farmland : MonoBehaviour
{
    public enum FarmlandState { Normal, Plowed, Watered }
    public UnityEvent OnWateringComplete;

    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite plowedSprite;
    [SerializeField] private Sprite[] wateredSprites;
    [SerializeField] private float waterAnimationSpeed = 0.2f;

    

    private SpriteRenderer spriteRenderer;
    private FarmlandState currentState = FarmlandState.Normal;
    private Vector3Int currentTilePos;
    private Coroutine waterAnimationCoroutine;

    public FarmlandState CurrentState => currentState;
    public Vector3Int CurrentTilePos => currentTilePos;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) Debug.LogError($"Farmland {gameObject.name} is missing SpriteRenderer!");
        SetStateVisuals(FarmlandState.Normal);

        Tilemap walkableTilemap = GridManager.Instance?.walkableTilemap;
        if (walkableTilemap != null && GridManager.Instance != null)
        {
            currentTilePos = walkableTilemap.WorldToCell(transform.position);
            GridManager.Instance.OccupyTile(currentTilePos, gameObject);
        }
        else Debug.LogError($"[Farmland] GridManager or Walkable Tilemap not found for {gameObject.name}!");
    }


    public void Hoe()
    {
        if (currentState == FarmlandState.Normal)
        {
            currentState = FarmlandState.Plowed;
            SetStateVisuals(FarmlandState.Plowed);
 
        }
    }

    public void Water()
    {
        if (currentState == FarmlandState.Plowed)
        {
            currentState = FarmlandState.Watered;
            SetStateVisuals(FarmlandState.Watered);
        }
    }


    public void RestoreState(FarmlandState stateToRestore)
    {
        currentState = stateToRestore;
        SetStateVisuals(stateToRestore);
    }


    private void SetStateVisuals(FarmlandState state)
    {
        if (spriteRenderer == null) return;

        if (waterAnimationCoroutine != null)
        {
            StopCoroutine(waterAnimationCoroutine);
            waterAnimationCoroutine = null;
        }

        switch (state)
        {
            case FarmlandState.Normal:
                spriteRenderer.sprite = normalSprite;
                break;
            case FarmlandState.Plowed:
                spriteRenderer.sprite = plowedSprite;
                break;
            case FarmlandState.Watered:
                if (wateredSprites != null && wateredSprites.Length > 0)
                {
                    waterAnimationCoroutine = StartCoroutine(PlayWaterAnimationAndInvokeEvent()); // 이름 변경
                }
                else
                {
                    // ... (애니메이션 스프라이트 없을 때 처리)
                    // 이벤트 즉시 발생?
                    // OnWateringComplete?.Invoke();
                }
                break;
        }
    }

    private IEnumerator PlayWaterAnimationAndInvokeEvent()
    {
        int totalFrames = wateredSprites.Length;
        for (int i = 0; i < totalFrames; i++)
        {
            if (spriteRenderer == null) yield break;
            spriteRenderer.sprite = wateredSprites[i];
            yield return new WaitForSeconds(waterAnimationSpeed);
        }

        waterAnimationCoroutine = null;

        Debug.Log("Invoking OnWateringComplete event...");
        OnWateringComplete?.Invoke(); 
    }


    private IEnumerator PlayWaterAnimation()
    {
        int currentIndex = 0;
        while (true) 
        {
            if (spriteRenderer == null || wateredSprites == null || wateredSprites.Length == 0) yield break;

            spriteRenderer.sprite = wateredSprites[currentIndex];
            currentIndex = (currentIndex + 1) % wateredSprites.Length; 
            yield return new WaitForSeconds(waterAnimationSpeed);
        }
    }

    //오브젝트 파괴시...
    private void OnDestroy()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.VacateTile(currentTilePos);
        }
    }
}