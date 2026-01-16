using TMPro;
using UnityEngine;

public class ToastMessageView : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    private RectTransform rectTransform;

    private float hideTime;
    private UIAnimationManager anim => GameManager.UI.GetAnimationManager(); // UIManager에서 꺼내쓰자

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public bool IsActive => gameObject.activeSelf;

    public void Show(string message, float duration = 2f)
    {
        messageText.text = message;
        gameObject.SetActive(true);

        anim.PlayFadeInAndPop(rectTransform, 0.3f);
        hideTime = Time.time + duration;

        CancelInvoke();
        Invoke(nameof(Hide), duration);
    }

    public float GetRemainingTime()
    {
        return hideTime - Time.time;
    }

    private void Hide()
    {
        anim.PlayFadeOut(rectTransform, 0.3f);
    }
}
