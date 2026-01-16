using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Tilemaps;
using System;

public class StageManager : MonoBehaviour
{
    [SerializeField] private TileManager tileManager;
    [SerializeField] private StageUIView stageUiView;
    [SerializeField] private StageRewardManager stageRewardManager;
    [SerializeField] private Player player;
    [SerializeField] private Tilemap walkableTilemap;

    [Header("Movement Buttons")]
    [SerializeField] private Button buttonUp;
    [SerializeField] private Button buttonDown;
    [SerializeField] private Button buttonLeft;
    [SerializeField] private Button buttonRight;

    [Header("Undo/Redo Buttons")]
    [SerializeField] private Button buttonUndo;
    [SerializeField] private Button buttonRedo;

    [Header("Action Button")]
    [SerializeField] private Button buttonAction;


    public Player Player => player;

    private bool _isClearing;

    private void Start()
    {
        InitStage(GameManager.Backend.GameData.UserData.PresentStageKey);
    }

    private void InitStage(int stageKey)
    {
        Debug.Log($">> 받은 스테이지 키: {stageKey}");

        tileManager.TileInit(stageKey, this);
        player.PlayerInit();
        stageUiView.SetStageName(GameManager.GameScene.StageName);
        SetupButtonListeners();
        SetupWalkableTilemap();
        SetupUndoRedoButtons();
    }

    private void SetupButtonListeners()
    {
        if (player.PlayerController == null)
        {
            Debug.LogError("컨트롤러없음");
            return;
        }

        buttonUp?.onClick.AddListener(() => player.PlayerController.MoveUp());
        buttonDown?.onClick.AddListener(() => player.PlayerController.MoveDown());
        buttonLeft?.onClick.AddListener(() => player.PlayerController.MoveLeft());
        buttonRight?.onClick.AddListener(() => player.PlayerController.MoveRight());
        buttonAction.onClick.AddListener(() => player.PlayerController.PerformAction());

        stageUiView.OnStageOutButtonClick += OnStageOutButtonClicked;
        stageUiView.OnStageClearExitClick += OnStageClearOutButtonClicked;

        stageUiView.OnStagePauseButtonClick += OnStagePauseButtonClicked;
    }

    private void OnDisable()
    {
        stageUiView.OnStageOutButtonClick -= OnStageOutButtonClicked;
        stageUiView.OnStageClearExitClick -= OnStageClearOutButtonClicked;

        stageUiView.OnStagePauseButtonClick -= OnStagePauseButtonClicked;
    }

    private void SetupWalkableTilemap()
    {
        if (GridManager.Instance != null)
        {
            if (walkableTilemap != null)
            {
                GridManager.Instance.SetWalkableTilemap(walkableTilemap);
            }
            else
            {
                Debug.LogError("타일맵없음");
            }
        }
        else
        {
            Debug.LogError("그리드매니저없음");
        }
    }

    private void SetupUndoRedoButtons()
    {
        buttonUndo?.onClick.AddListener(() => UndoManager.Instance.Undo());
        buttonRedo?.onClick.AddListener(() => UndoManager.Instance.Redo());
    }

    public async Task GameClear()
    {
        if (_isClearing) return; 
        _isClearing = true;

        try
        {
            int stageKey = GameManager.Backend.GameData.UserData.PresentStageKey;
            var userData = GameManager.Backend.GameData.UserData;

            Debug.Log($"[GameClear]: stageKey={stageKey}");

            float prevExp = userData.Exp;
            float prevMaxExp = userData.MaxExp;

            GameManager.Backend.GameData.StageAchievement.SetAchieve(stageKey);
            GameManager.Backend.GameData.UserData.UpdateStageClear(stageKey);

            var reward = stageRewardManager.GetRewardData(stageKey);
            if (reward == null)
            {
                Debug.LogError("보상 정보 없음");
                return;
            }

            await stageRewardManager.GrantStageClearRewardAsync(stageKey);

            float newMaxExp = userData.MaxExp;

            stageUiView.SetExpAmountText((int)userData.Exp, (int)userData.MaxExp);
            stageUiView.SetExpGetAmountText((int)reward.ExpReward);
            stageUiView.SetExpBar(prevExp, reward.ExpReward, prevMaxExp, newMaxExp);

            GameManager.Backend.GameData.UserHeart.UseHeart();
            GameManager.Backend.UpdateAllGameData(null);

            stageUiView.SetStageClearPanel(true);
        }
        finally
        {
            _isClearing = false;
        }
    }

    public void OnStagePauseButtonClicked()
    {
        stageUiView.SetStagePausePanel(true);
    }

    public void OnStageOutButtonClicked()
    {
        GameManager.GameScene.LoadSceneImmediate(SceneNameEnum.Lobby);
    }

    public void OnStageClearOutButtonClicked()
        => GameManager.GameScene.LoadSceneImmediate(SceneNameEnum.Lobby);
}
