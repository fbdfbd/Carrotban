using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class UIButtonSFX : MonoBehaviour
{
    public enum SfxType { Menu, Stage }

    [SerializeField] private SfxType type = SfxType.Menu;
    [SerializeField] private string menuSfx = "Click_Menu";
    [SerializeField] private string stageSfx = "Click_Stage";
    [SerializeField] private bool randomizePitch = false;
    [SerializeField] private float pitchMin = 0.95f;
    [SerializeField] private float pitchMax = 1.05f;

    private Button _btn;
    private bool _boundOnce;

    void Awake()
    {
        _btn = GetComponent<Button>();
    }

    void OnEnable()
    {
        StartCoroutine(RebindNextFrame());
    }

    void OnDisable()
    {
        if (_btn != null) _btn.onClick.RemoveListener(OnClickPlaySFX);
    }

    private IEnumerator RebindNextFrame()
    {
        if (_btn != null) _btn.onClick.RemoveListener(OnClickPlaySFX);

        yield return null;

        if (!isActiveAndEnabled || _btn == null) yield break;

        _btn.onClick.AddListener(OnClickPlaySFX);
        _boundOnce = true;
    }

    private void OnClickPlaySFX()
    {
        var sm = GameManager.SoundManager;
        if (sm == null) return;

        string clip = (type == SfxType.Menu) ? menuSfx : stageSfx;
        float pitch = randomizePitch ? Random.Range(pitchMin, pitchMax) : 1f;
        sm.PlaySFX(clip, pitch);
    }
}
