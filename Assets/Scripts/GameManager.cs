using UnityEngine;
using System.Collections;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static BackendManager Backend { get; private set; }
    public static UIManager UI { get; private set; }
    public static GameSceneManager GameScene { get; private set; }

    public static SoundManager SoundManager { get; private set; }
    
    public GameLanguage Language = GameLanguage.KOR;

    public DateTime ServerDateTime { get; private set; }

    public static event Action WalletChanged;
    public static void RaiseWalletChanged() => WalletChanged?.Invoke();

    public void UpdateServerDateTime()
    {
        ServerDateTime = BackendData.Utils.BackendUtils.GetServerDateTime();
    }

    void Awake()
    {
        Debug.Log("[GameManager] Awake 시작");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] Instance 설정 및 DontDestroyOnLoad 완료");
        }
        else
        {
            Debug.LogWarning($"[GameManager] 중복된 GameManager 인스턴스 발견 ({gameObject.name}). 이 인스턴스를 파괴합니다.");
            Destroy(gameObject);
            return;
        }
        
        Application.targetFrameRate = -1;
        AssignManagerReferences();
        QualitySettings.vSyncCount = 0;                
        Application.targetFrameRate = 60;               
        UnityEngine.Rendering.OnDemandRendering.renderFrameInterval = 1;
    }

    private void AssignManagerReferences()
    {
        Backend = GetComponentInChildren<BackendManager>(true);
        UI = GetComponentInChildren<UIManager>(true);
        GameScene = GetComponentInChildren<GameSceneManager>(true);
        SoundManager = GetComponentInChildren<SoundManager>(true);
    }

    void Start()
    {
        StartCoroutine(InitializeGameRoutine());
    }

    private IEnumerator InitializeGameRoutine() 
    {
        UI.Init();
        Backend.Init();
        SoundManager.Init();

        Debug.Log($"[GameManager] 모든 초기화 완료.씬으로 전환합니다.");
        if (GameScene != null)
        {
            GameScene.LoadSceneImmediate(SceneNameEnum.Title);
        }
        else
        {
            Debug.LogError("참조없음");
        }
        yield break;
    }
}