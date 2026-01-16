using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    [Header("Volumes")]
    [Range(0f, 1f)] public float bgmVolume = 0.8f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;

    private const string KEY_BGM_ON = "opt.bgm.on";
    private const string KEY_SFX_ON = "opt.sfx.on";
    private const string KEY_BGM_VOL = "opt.bgm.vol";
    private const string KEY_SFX_VOL = "opt.sfx.vol";

    private AudioSource bgmSrc;
    private AudioSource sfxSrc;

    private readonly Dictionary<string, AudioClip> bgm = new();
    private readonly Dictionary<string, AudioClip> sfx = new();

    public bool IsBGMOn { get; private set; } = true;
    public bool IsSFXOn { get; private set; } = true;

    public void Init()
    {
        bgmSrc = gameObject.AddComponent<AudioSource>();
        bgmSrc.loop = true;
        bgmSrc.playOnAwake = false;

        sfxSrc = gameObject.AddComponent<AudioSource>();
        sfxSrc.loop = false;
        sfxSrc.playOnAwake = false;

        foreach (var c in Resources.LoadAll<AudioClip>("Audio/BGM"))
            if (!bgm.ContainsKey(c.name)) bgm.Add(c.name, c);
        foreach (var c in Resources.LoadAll<AudioClip>("Audio/SFX"))
            if (!sfx.ContainsKey(c.name)) sfx.Add(c.name, c);

        IsBGMOn = PlayerPrefs.GetInt(KEY_BGM_ON, 1) == 1;
        IsSFXOn = PlayerPrefs.GetInt(KEY_SFX_ON, 1) == 1;
        bgmVolume = PlayerPrefs.GetFloat(KEY_BGM_VOL, bgmVolume);
        sfxVolume = PlayerPrefs.GetFloat(KEY_SFX_VOL, sfxVolume);

        ApplyVolumes();
        ApplyMutes();
        EnsureAudioListener();

        Debug.Log($"[Sound] Init | BGM:{bgm.Count} SFX:{sfx.Count} | On(B:{IsBGMOn},S:{IsSFXOn}) Vol(B:{bgmVolume:F2},S:{sfxVolume:F2})");
        foreach (var k in bgm.Keys) Debug.Log($"[Sound] BGM Loaded: {k}");
        foreach (var k in sfx.Keys) Debug.Log($"[Sound] SFX Loaded: {k}");
    }


    private void EnsureAudioListener()
    {
        var listener = FindObjectOfType<AudioListener>();
        if (listener == null)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.gameObject.AddComponent<AudioListener>();
                Debug.LogWarning("[Sound] No AudioListener found. Added one to Main Camera.");
            }
            else
            {
                Debug.LogWarning("[Sound] No AudioListener & no Main Camera found.");
            }
        }
    }

    private void ApplyVolumes()
    {
        if (bgmSrc != null) bgmSrc.volume = bgmVolume;
        if (sfxSrc != null) sfxSrc.volume = sfxVolume;
    }

    private void ApplyMutes()
    {
        if (bgmSrc != null) bgmSrc.mute = !IsBGMOn;
        if (sfxSrc != null) sfxSrc.mute = !IsSFXOn;
    }

    public void SetBGMOnOff(bool on)
    {
        IsBGMOn = on;
        PlayerPrefs.SetInt(KEY_BGM_ON, on ? 1 : 0);
        ApplyMutes();
        PlaySFX("Click_Menu");
    }

    public void SetSFXOnOff(bool on)
    {
        IsSFXOn = on;
        PlayerPrefs.SetInt(KEY_SFX_ON, on ? 1 : 0);
        ApplyMutes();
        if (on) PlaySFX("Click_Menu");
    }

    public void SetBGMVolume(float v)
    {
        bgmVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KEY_BGM_VOL, bgmVolume);
        ApplyVolumes();
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KEY_SFX_VOL, sfxVolume);
        ApplyVolumes();
    }

    public bool HasBGM(string name) => bgm.ContainsKey(name);

    public void PlayTitleOrLobbyBGM() => PlayBGM("Title"); // °ø¿ë

    public void PlayStageBGMByChapter(int chapterId)
    {
        string key = $"Stage{chapterId}";
        if (!HasBGM(key))
        {
            Debug.LogWarning($"[Sound] Missing BGM '{key}', fallback to Title");
            key = "Title";
        }
        PlayBGM(key);
    }

    public void PlayBGM(string name, float fade = 0.25f)
    {
        if (!bgm.TryGetValue(name, out var clip))
        {
            Debug.LogWarning($"[Sound] BGM '{name}' not found in dict.");
            return;
        }

        EnsureAudioListener();
        ApplyMutes();   
        ApplyVolumes(); 

        if (bgmSrc.clip == clip && bgmSrc.isPlaying)
        {
            Debug.Log($"[Sound] BGM '{name}' already playing (mute:{bgmSrc.mute}, vol:{bgmSrc.volume:F2})");
            return;
        }

        Debug.Log($"[Sound] PlayBGM '{name}' (mute:{bgmSrc.mute}, vol:{bgmSrc.volume:F2})");
        StopAllCoroutines();
        StartCoroutine(FadeAndPlayBGM(clip, fade));
    }

    public void StopBGM(float fade = 0.2f)
    {
        StopAllCoroutines();
        if (!bgmSrc.isPlaying) return;
        StartCoroutine(FadeOutBGM(fade));
    }

    public void PlaySFX(string name, float pitch = 1f)
    {
        if (!IsSFXOn) return;
        if (!sfx.TryGetValue(name, out var clip))
        {
            Debug.LogWarning($"[Sound] SFX '{name}' not found.");
            return;
        }

        EnsureAudioListener();
        ApplyMutes();
        ApplyVolumes();

        sfxSrc.pitch = pitch;
        sfxSrc.PlayOneShot(clip, sfxVolume);
    }

    private System.Collections.IEnumerator FadeAndPlayBGM(AudioClip clip, float fade)
    {
        float t = 0f; float start = bgmSrc.volume;
        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            bgmSrc.volume = Mathf.Lerp(start, 0f, t / fade);
            yield return null;
        }
        bgmSrc.Stop();
        bgmSrc.clip = clip;
        bgmSrc.Play();

        t = 0f;
        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            bgmSrc.volume = Mathf.Lerp(0f, bgmVolume, t / fade);
            yield return null;
        }
        bgmSrc.volume = bgmVolume;
    }

    private System.Collections.IEnumerator FadeOutBGM(float fade)
    {
        float t = 0f; float start = bgmSrc.volume;
        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            bgmSrc.volume = Mathf.Lerp(start, 0f, t / fade);
            yield return null;
        }
        bgmSrc.Stop();
        bgmSrc.clip = null;
        bgmSrc.volume = bgmVolume;
    }
}
