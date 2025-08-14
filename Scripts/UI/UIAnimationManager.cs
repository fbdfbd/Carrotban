using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class UIAnimationManager 
{
    private RectTransform _mainCanvasRectTransform;
    private Camera _mainCamera;

    public UIAnimationManager(RectTransform canvasRectTransform, Camera initialCamera)
    {
        _mainCanvasRectTransform = canvasRectTransform;
        _mainCamera = initialCamera;

        if (_mainCamera == null)
        {
            Debug.LogWarning("<color=yellow>[UIAnimationManager]</color> �ʱ� ī�޶� null�Դϴ�. Camera.main�� ����Ϸ��� �õ��մϴ�.");
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogError("<color=red>[UIAnimationManager]</color> Camera.main�� ã�� �� �����ϴ�! ī�޶� ������ Ȯ�����ּ���.");
            }
        }
    }

    public void UpdateCamera(Camera newCamera)
    {
        if (newCamera != null)
        {
            _mainCamera = newCamera;
            Debug.Log($"<color=cyan>[UIAnimationManager]</color> ī�޶� '{newCamera.name}' (��)�� ������Ʈ�Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("<color=yellow>[UIAnimationManager]</color> ������Ʈ �Ϸ��� �� ī�޶� null�Դϴ�.");
        }
    }


    public void PlayShakeAnimation(RectTransform targetRect, float duration = 0.5f, float strength = 10f, int vibrato = 10, float randomness = 90f, bool fadeOut = true)
    {
        if (targetRect == null)
        {
            Debug.LogError("<color=red>[UIAnimationManager]</color> ��� RectTransform�� null�Դϴ�.");
            return;
        }
        targetRect.DOShakeAnchorPos(duration, strength, vibrato, randomness, true, fadeOut);
    }

    public void PlayFadeInAndPop(RectTransform target, float duration = 0.5f, float popHeight = 30f)
    {
        if (target == null) return;

        target.localScale = Vector3.zero;
        target.anchoredPosition += Vector2.up * popHeight;

        Sequence seq = DOTween.Sequence();
        seq.Append(target.DOScale(Vector3.one, duration).SetEase(Ease.OutBack));
        seq.Join(target.DOAnchorPosY(target.anchoredPosition.y - popHeight, duration).SetEase(Ease.OutCubic));

        CanvasGroup group = GetOrAddCanvasGroup(target.gameObject);
        group.alpha = 0;
        seq.Join(group.DOFade(1, duration * 0.8f));
    }

    public void PlayFadeOut(RectTransform target, float duration = 0.5f)
    {
        if (target == null) return;

        CanvasGroup group = GetOrAddCanvasGroup(target.gameObject);
        group.DOFade(0, duration).OnComplete(() =>
        {
            target.gameObject.SetActive(false);
        });
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        CanvasGroup group = go.GetComponent<CanvasGroup>();
        if (group == null) group = go.AddComponent<CanvasGroup>();
        return group;
    }
    public void AnimateExpBarByExpGain(
        Slider expBar,
        float prevExp,
        float addedExp,
        float prevMaxExp,
        float newMaxExp)
    {
        if (expBar == null)
        {
            Debug.LogError("[UIAnimationManager] ExpBar �����̴��� null�Դϴ�!");
            return;
        }


        expBar.maxValue = prevMaxExp; 
        expBar.value = prevExp;

        Sequence seq = DOTween.Sequence();
        float overflowExp = prevExp + addedExp;

        if (overflowExp >= prevMaxExp)
        {
            seq.Append(expBar.DOValue(prevMaxExp, 0.5f));

            seq.AppendCallback(() =>
            {
                expBar.maxValue = newMaxExp; 
                expBar.value = 0f;
            });

            float remain = overflowExp - prevMaxExp;
            seq.Append(expBar.DOValue(remain, 0.5f));
        }
        else
        {
            seq.Append(expBar.DOValue(overflowExp, 0.5f));
        }
    }

}