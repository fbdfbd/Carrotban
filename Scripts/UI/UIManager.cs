using UnityEngine;
using UnityEngine.UI;           
using DG.Tweening;              
using System.Collections.Generic; 
using UnityEngine.SceneManagement; 

public class UIManager : MonoBehaviour
{
    [SerializeField] private RectTransform mainCanvasRectTransform;
    [SerializeField] private Camera initialMainCamera; 
    [SerializeField] private List<GameObject> uiViewPrefabs;
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
                Debug.LogWarning("<color=yellow>[UIManager]</color> Awake���� �ʱ� ī�޶� ã�� ���߽��ϴ�.");
            }
            Debug.Log("<color=lime>[UIManager]</color> UIAnimationManager ���񽺰� ���������� �ʱ�ȭ�Ǿ����ϴ�. " +
                      (cameraForAnimation != null ? $"�ʱ� ī�޶�: {cameraForAnimation.name}" : "ī�޶� ���� �ʿ�"));
        }
        else
        {
            Debug.LogError("<color=red>[UIManager]</color> UIAnimationManager �ʱ�ȭ ����! �ֿ� ���� ���� Ȯ�� �ʿ�.");
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
                Debug.LogWarning($"<color=yellow>[UIManager]</color> �� '{scene.name}'���� MainCamera �±׸� ���� ī�޶� ã�� �� �����ϴ�. UI �ִϸ��̼��� ���� ī�޶� ������ ��� ����� �� �ֽ��ϴ�.");
            }
        }
        else
        {
            Debug.LogError("<color=red>[UIManager]</color> UIAnimationManager�� �ʱ�ȭ���� �ʾҽ��ϴ�. OnSceneLoaded���� ī�޶� ������Ʈ�� �� �����ϴ�.");
        }
    }

    public void ShowLoadingScreen(string sceneName = null)
    {
        // loadingScreenPanel.SetActive(true);
        // if (loadingSceneNameText != null && !string.IsNullOrEmpty(sceneName))
        // {
        //     loadingSceneNameText.text = $"Loading {sceneName}...";
        // }
        // UpdateLoadingProgress(0); // �ʱ� ����� 0���� ����
        Debug.Log($"[UIManager] �ε� ȭ�� ǥ�� (���: {sceneName ?? "N/A"})");
    }

    public void UpdateLoadingProgress(float progress)
    {
        // if (loadingProgressBar != null)
        // {
        //     loadingProgressBar.value = progress;
        // }
        // Debug.Log($"[UIManager] �ε� �����: {progress * 100:F0}%");
    }

    public void FadeOut(float duration)
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("[UIManager] fadeCanvasGroup�� �������� �ʾҽ��ϴ�.");
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
            Debug.LogError("[UIManager] fadeCanvasGroup�� �������� �ʾҽ��ϴ�.");
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

    // --- �� �ε� ���� UI �޼��� ���� ---
    public void ShowLoadingScreen()
    {
        Debug.Log("<color=cyan>[UIManager]</color> �ε� ȭ�� ǥ��.");
    }

    public void HideLoadingScreen()
    {
        Debug.Log("<color=cyan>[UIManager]</color> �ε� ȭ�� ����.");
    }

    public void ShowToast(string msg, float duration = 2f)
    {
        if (toastPool.Count == 0)
        {
            Debug.LogError("[UIManager] ToastMessageView 5���־�");
            return;
        }

        ToastMessageView availableToast = toastPool.Find(t => !t.IsActive);
        Debug.Log($"[ToastMessageView] Show: {msg}");
        if (availableToast != null)
        {
            availableToast.transform.SetAsLastSibling();
            availableToast.Show(msg, duration);
            Debug.Log("�佺Ʈǩ��");
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
                Debug.Log("�佺Ʈǩ��");
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