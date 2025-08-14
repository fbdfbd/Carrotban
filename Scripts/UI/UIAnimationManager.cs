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
            Debug.LogWarning("<color=yellow>[UIAnimationManager]</color> 초기 카메라가 null입니다. Camera.main을 사용하려고 시도합니다.");
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogError("<color=red>[UIAnimationManager]</color> Camera.main도 찾을 수 없습니다! 카메라 참조를 확인해주세요.");
            }
        }
    }

    public void UpdateCamera(Camera newCamera)
    {
        if (newCamera != null)
        {
            _mainCamera = newCamera;
            Debug.Log($"<color=cyan>[UIAnimationManager]</color> 카메라가 '{newCamera.name}' (으)로 업데이트되었습니다.");
        }
        else
        {
            Debug.LogWarning("<color=yellow>[UIAnimationManager]</color> 업데이트 하려는 새 카메라가 null입니다.");
        }
    }


    public void PlayShakeAnimation(RectTransform targetRect, float duration = 0.5f, float strength = 10f, int vibrato = 10, float randomness = 90f, bool fadeOut = true)
    {
        if (targetRect == null)
        {
            Debug.LogError("<color=red>[UIAnimationManager]</color> 흔들 RectTransform이 null입니다.");
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
            Debug.LogError("[UIAnimationManager] ExpBar 슬라이더가 null입니다!");
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