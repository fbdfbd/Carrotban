using UnityEngine;

public class LobbyOptionManager : MonoBehaviour
{
    [SerializeField] private LobbyUIOptionView optionView;

    public void Init()
    {
        if (optionView != null && GameManager.SoundManager != null)
        {
            optionView.SetSoundToggles(GameManager.SoundManager.IsBGMOn, GameManager.SoundManager.IsSFXOn);

            optionView.OnBGMToggleValueChanged += HandleBGMToggleChanged;
            optionView.OnSFXToggleValueChanged += HandleSFXToggleChanged;
        }
    }

    private void HandleBGMToggleChanged(bool isOn)
    {
        GameManager.SoundManager?.SetBGMOnOff(isOn);
    }

    private void HandleSFXToggleChanged(bool isOn)
    {
        GameManager.SoundManager?.SetSFXOnOff(isOn);
    }
}
