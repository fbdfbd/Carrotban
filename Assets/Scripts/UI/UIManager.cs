using UnityEngine;
using UnityEngine.UI;           
using DG.Tweening;              
using System.Collections.Generic; 
using UnityEngine.SceneManagement; 

public class UIManager : MonoBehaviour
{
    [SerializeField] private RectTransform mainCanvasRectTransform;
    [SerializeField] private Camera initialMainCamera; 
    [SerializeField] private Transform canvasTransform;
    [SerializeField] private CanvasGroup fadeCanvasGroup;


    [SerializeField] private List<ToastMessageView> toastPool = new List<ToastMessageView>();
    private const int MaxToastCount = 5;

    private UIAnimationManager uiAnimationManager;
    private Stack<GameObject> uiStack = new Stack<GameObject>();

    public UIAnimationManager GetAnimationManager() => uiAnimationManager;


    public void Init()
    {
        Camera cameraForAnimation = initialMainCamera != null ? initialMainCamera : Camera.main;
        if (mainCanvasRectTransform != null && canvasTransform != null)
        {
            uiAnimationManager = new UIAnimationManager(mainCanvasRectTransform, cameraForAnimation);
            if (cameraForAnimation == null)
            {
                Debug.LogWarning("<color=yellow>[UIManager]</color> Awake에서 초기 카메라를 찾지 못했습니다.");
            }
            Debug.Log("<color=lime>[UIManager]</color> UIAnimationManager 서비스가 성공적으로 초기화되었습니다. " +
                      (cameraForAnimation != null ? $"초기 카메라: {cameraForAnimation.name}" : "카메라 설정 필요"));
        }
        else
        {
            Debug.LogError("<color=red>[UIManager]</color> UIAnimationManager 초기화 실패! 주요 참조 연결 확인 필요.");
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Camera currentSceneMainCamera = Camera.main;

        if (uiAnimationManager != null)
        {
            if (currentSceneMainCamera != null)
            {
                uiAnimationManager.UpdateCamera(currentSceneMainCamera);
            }
            else
            {
                Debug.LogWarning($"<color=yellow>[UIManager]</color> 씬 '{scene.name}'에서 MainCamera 태그를 가진 카메라를 찾을 수 없습니다. UI 애니메이션이 이전 카메라 설정을 계속 사용할 수 있습니다.");
            }
        }
        else
        {
            Debug.LogError("<color=red>[UIManager]</color> UIAnimationManager가 초기화되지 않았습니다. OnSceneLoaded에서 카메라를 업데이트할 수 없습니다.");
        }
    }

    public void FadeOut(float duration)
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("[UIManager] fadeCanvasGroup이 설정되지 않았습니다.");
            return;
        }
        var go = fadeCanvasGroup.gameObject;
        if (!go.activeSelf) go.SetActive(true);

        fadeCanvasGroup.DOKill(true);
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = true;  
        fadeCanvasGroup.interactable = false;  

        fadeCanvasGroup
            .DOFade(1f, duration)
            .SetUpdate(true)                  
            .SetEase(Ease.Linear);
    }

    public void FadeIn(float duration)
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("[UIManager] fadeCanvasGroup이 설정되지 않았습니다.");
            return;
        }

        var go = fadeCanvasGroup.gameObject;
        if (!go.activeSelf) go.SetActive(true); 

        fadeCanvasGroup.DOKill(true);
        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.blocksRaycasts = true; 
        fadeCanvasGroup.interactable = false;

        fadeCanvasGroup
            .DOFade(0f, duration)
            .SetUpdate(true)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                fadeCanvasGroup.blocksRaycasts = false;
                go.SetActive(false);
            });
    }

    // --- 씬 로딩 관련 UI 메서드 예시 ---
    public void ShowLoadingScreen()
    {
        Debug.Log("<color=cyan>[UIManager]</color> 로딩 화면 표시.");
    }

    public void HideLoadingScreen()
    {
        Debug.Log("<color=cyan>[UIManager]</color> 로딩 화면 숨김.");
    }

    public void ShowToast(string msg, float duration = 2f)
    {
        if (toastPool.Count == 0)
        {
            Debug.LogError("[UIManager] ToastMessageView 5개넣어");
            return;
        }

        ToastMessageView availableToast = toastPool.Find(t => !t.IsActive);
        Debug.Log($"[ToastMessageView] Show: {msg}");
        if (availableToast != null)
        {
            availableToast.transform.SetAsLastSibling();
            availableToast.Show(msg, duration);
            Debug.Log("토스트푯기");
        }
        else
        {
            ToastMessageView oldest = null;
            float maxRemaining = float.MinValue;

            foreach (var toast in toastPool)
            {
                float remaining = toast.GetRemainingTime();
                if (remaining > maxRemaining)
                {
                    maxRemaining = remaining;
                    oldest = toast;
                }
            }

            if (oldest != null)
            {
                oldest.transform.SetAsLastSibling();
                oldest.Show(msg, duration);
                Debug.Log("토스트푯기");
            }
        }
    }
    public void PlayFadeInAndPop(RectTransform target, float duration = 0.35f, float popHeight = 40f)
    {
        if (target == null) return;

        target.DOKill(true);
        var group = GetOrAddCanvasGroup(target.gameObject);
        group.DOKill(true);
        DOTween.Kill(target.gameObject);

        LayoutRebuilder.ForceRebuildLayoutImmediate(target);

        Vector2 origPos = target.anchoredPosition;
        target.localScale = Vector3.zero;
        target.anchoredPosition = origPos + Vector2.up * popHeight;
        group.alpha = 0f;


        Sequence seq = DOTween.Sequence()
            .SetUpdate(true)
            .SetLink(target.gameObject, LinkBehaviour.KillOnDestroy)
            .Append(target.DOScale(1f, duration).SetEase(Ease.OutBack))
            .Join(target.DOAnchorPos(origPos, duration).SetEase(Ease.OutCubic))
            .Join(group.DOFade(1f, duration * 0.8f));
    }

    public void PlayFadeOut(RectTransform target, float duration = 0.25f)
    {
        if (target == null) return;

        target.DOKill(true);
        var group = GetOrAddCanvasGroup(target.gameObject);
        group.DOKill(true);
        DOTween.Kill(target.gameObject);

        Sequence seq = DOTween.Sequence()
            .SetUpdate(true)
            .SetLink(target.gameObject, LinkBehaviour.KillOnDestroy)
            .Append(group.DOFade(0f, duration))
            .OnComplete(() =>
            {
                target.gameObject.SetActive(false);
            });
    }
    private CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        var group = go.GetComponent<CanvasGroup>();
        if (group == null) group = go.AddComponent<CanvasGroup>();
        return group;
    }
}