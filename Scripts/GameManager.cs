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
        Debug.Log("[GameManager] Awake ����");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] Instance ���� �� DontDestroyOnLoad �Ϸ�");
        }
        else
        {
            Debug.LogWarning($"[GameManager] �ߺ��� GameManager �ν��Ͻ� �߰� ({gameObject.name}). �� �ν��Ͻ��� �ı��մϴ�.");
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

        Debug.Log($"[GameManager] ��� �ʱ�ȭ �Ϸ�.������ ��ȯ�մϴ�.");
        if (GameScene != null)
        {
            GameScene.LoadSceneImmediate(SceneNameEnum.Title);
        }
        else
        {
            Debug.LogError("��������");
        }
        yield break;
    }
}