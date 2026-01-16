using BackEnd;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private LobbyShopManager lobbyShopManager;
    [SerializeField] private LobbyNoticeManager lobbyNoticeManager;
    [SerializeField] private LobbyPostManager lobbyPostManager;
    [SerializeField] private LobbyQuestManager lobbyQuestManager;
    [SerializeField] private LobbyOptionManager lobbyOptionManager;

    [SerializeField] private LobbyUIView lobbyUIView;
    [SerializeField] private LobbyUIProfileView lobbyUIProfileView;
    [SerializeField] private LobbyUIOptionView lobbyUIOptionView;

    public class ChapterButtonInfo
    {
        public int Id;
        public string Name_KR;
        public string Name_ENG;
        public bool IsUnlocked;
    }

    public class StageButtonInfo
    {
        public int StageKey;
        public bool IsUnlocked;
    }

    // PresentKey는 표시 CurrentKey는 해금
    private void Start()
    {
        GameManager.Instance.UpdateServerDateTime();
        GameManager.Instance.Language = GameManager.Backend.GameData.UserData.Language;
        HeartTimer.OnTimerFinishedEvent += HandleHeartRegen;

        Init();

        lobbyShopManager.Init();
        lobbyNoticeManager.Init();
        lobbyPostManager.Init();
        lobbyQuestManager.Init();
        lobbyOptionManager.Init();

        lobbyUIView.Init();
        lobbyUIProfileView.Init();
        lobbyUIOptionView.Init();

        RegisterUIViewEventHandlers();
        RegisterUIProfileViewEventHandlers();
        RegisterUIOptionViewEventHandlers();

        GameManager.SoundManager?.PlayTitleOrLobbyBGM();

        GameManager.WalletChanged += OnWalletChanged;
    }

    private void OnWalletChanged()
    {
        int heart = GameManager.Backend.GameData.UserHeart.Heart;

        lobbyUIView.SetGold(GameManager.Backend.GameData.UserData.Gold);
        lobbyUIView.SetGem(GameManager.Backend.GameData.UserGem.Gem);
        lobbyUIView.SetHeart(heart);
        lobbyUIProfileView.SetLevel(GameManager.Backend.GameData.UserData.Level);
        lobbyUIProfileView.SetExp(GameManager.Backend.GameData.UserData.Exp,
                                    GameManager.Backend.GameData.UserData.MaxExp);

        if(lobbyUIView.GetHeartTimer().gameObject.activeSelf)
        {
            if(heart >= 5) lobbyUIView.SetHeartTimer(false);
        }
    }

    private void OnDestroy()
    {
        #region Unregister Events
        GameManager.WalletChanged -= OnWalletChanged;

        if (lobbyUIView != null)
        {
            lobbyUIView.OnProfileButtonClick -= OnProfileButtonClicked;
            lobbyUIView.OnNoticeButtonClick -= OnNoticeButtonClicked;
            lobbyUIView.OnShopButtonClick -= OnShopButtonClicked;
            lobbyUIView.OnQuestButtonClick -= OnQuestButtonClicked;
            lobbyUIView.OnPostButtonClick -= OnPostButtonClicked;
            lobbyUIView.OnOptionButtonClick -= OnOptionButtonClicked;
            lobbyUIView.OnGemButtonClick -= OnGemButtonClicked;
            lobbyUIView.OnGoldButtonClick -= OnGoldButtonClicked;
            lobbyUIView.OnHeartButtonClick -= OnHeartButtonClicked;
            //lobbyUIView.OnCustomGameButtonClick -= OnCustomGameButtonClicked;
            lobbyUIView.OnStageButtonClick -= OnStageButtonClicked;
            lobbyUIView.OnChapterButtonClick -= OnChapterButtonClicked;
        }

        if (lobbyUIProfileView != null)
        {
            lobbyUIProfileView.OnProfileEditButtonClick -= OnProfileEditButtonClicked;
            lobbyUIProfileView.OnProfilePanelExitButtonClick -= OnProfilePanelExitButtonClicked;
            lobbyUIProfileView.OnProfileEditConfirmButtonClick -= OnProfileEditConfirmButtonClicked;
            lobbyUIProfileView.OnProfileEditCancleButtonClick -= OnProfileEditCancleButtonClicked;
        }

        if (lobbyUIOptionView != null)
        {
            lobbyUIOptionView.OnContactUsButtonClick -= OnContactUsButtonClicked;
            lobbyUIOptionView.OnBackendLoginButtonClick -= OnBackendLoginButtonClicked;
            lobbyUIOptionView.OnGoogleLoginButtonClick -= OnGoogleLoginButtonClicked;
            lobbyUIOptionView.OnExitButtonClick -= OnOptionPanelExitButtonClicked;
            lobbyUIOptionView.OnBGMToggleValueChanged -= OnBGMToggleValueChanged;
            lobbyUIOptionView.OnSFXToggleValueChanged -= OnSFXToggleValueChanged;
        }
        lobbyPostManager.OnPostChanged -= UpdatePostRedDot;
        lobbyQuestManager.OnQuestRedDotChanged -= UpdateQuestRedDot;

        HeartTimer.OnTimerFinishedEvent -= HandleHeartRegen;
        #endregion
    }

    private void HandleHeartRegen()
    {
        var uh = GameManager.Backend.GameData.UserHeart;

        uh.RefreshHeart();

        lobbyUIView.SetHeart(uh.Heart);

        if (uh.Heart >= uh.MaxHeart)
        {
            lobbyUIView.SetHeartTimer(false);
        }
        else
        {
            lobbyUIView.SetHeartTimer(true);
            int cycleSec = Mathf.Max(1, uh.RechargeMinutes) * 60;
            lobbyUIView.GetHeartTimer().StartTimer(cycleSec);
        }

        GameManager.Backend.UpdateAllGameData(callback =>
        {
            if (callback == null || callback.IsSuccess())
            {
                //충전되면 효과음 발생 할말
            }
            else
            {
                Debug.LogWarning($"하트 충전 저장 실패 {callback}");
                GameManager.UI.ShowToast("하트 저장 중 오류가 발생");
            }
        });
    }

    private void Init()
    {
        var userData = GameManager.Backend.GameData.UserData;

        GameManager.Backend.GameData.UserHeart.RefreshHeart();

        lobbyUIView.SetGold(userData.Gold);
        lobbyUIView.SetGem(GameManager.Backend.GameData.UserGem.Gem);
        lobbyUIView.SetHeart(GameManager.Backend.GameData.UserHeart.Heart);

        UpdateAllRedDot();

        var chapterInfos = BuildChapterButtonInfos();
        lobbyUIView.SetChapterButtons(chapterInfos);

        int presentChapter = GameManager.Backend.GameData.UserData.PresentStageKey / 100;

        OnChapterButtonClicked(presentChapter);

        lobbyUIView.SetNickname(Backend.UserNickName);
        lobbyUIProfileView.SetNickname(Backend.UserNickName);
        lobbyUIProfileView.SetLevel(GameManager.Backend.GameData.UserData.Level);
        lobbyUIProfileView.SetExp(GameManager.Backend.GameData.UserData.Exp,
                                    GameManager.Backend.GameData.UserData.MaxExp);
        lobbyUIProfileView.SetAchievedStageText();

        lobbyUIView.SetStageScrollViewPosition(GameManager.Backend.GameData.UserData.PresentStageKey);

        UpdateHeartTimer();
    }

    private void UpdateHeartTimer()
    {
        var uh = GameManager.Backend.GameData.UserHeart;

        if (uh.Heart >= uh.MaxHeart)
        {
            lobbyUIView.SetHeartTimer(false);
            return;
        }

        lobbyUIView.SetHeartTimer(true);

        int remainSec = (int)uh.GetNextHeartRemainTime().TotalSeconds;

        if (remainSec <= 0)
        {
            uh.RefreshHeart();

            lobbyUIView.SetHeart(uh.Heart);

            if (uh.Heart >= uh.MaxHeart)
            {
                lobbyUIView.SetHeartTimer(false);
                return;
            }

            remainSec = Mathf.Max(1, uh.RechargeMinutes * 60);

            GameManager.Backend.UpdateAllGameData(callback =>
            {
                if (!(callback == null || callback.IsSuccess()))
                {
                    Debug.LogWarning($"하트 초기화 저장 실패 {callback}");
                }
            });
        }
        lobbyUIView.GetHeartTimer().StartTimer(remainSec);
    }


    private void UpdateAllRedDot()
    {
        UpdateShopRedDot();
        UpdateProfileRedDot();
        UpdateOptionRedDot();
        UpdateNoticeRedDot();
        UpdatePostRedDot();
        UpdateQuestRedDot();
    }

    private List<ChapterButtonInfo> BuildChapterButtonInfos()
    {
        var dic = GameManager.Backend.Chart.Chapter.Dictionary;
        int unlockedChapter = GameManager.Backend.GameData.UserData.CurrentStageKey / 100;

        var infos = new List<ChapterButtonInfo>();
        foreach (var kv in dic)
        {
            var item = kv.Value;
            bool isUnlocked = item.ChapterID <= unlockedChapter;
            infos.Add(new ChapterButtonInfo
            {
                Id = item.ChapterID,
                Name_KR = item.Name_KR,
                Name_ENG = item.Name_ENG,
                IsUnlocked = isUnlocked
            });
        }
        infos.Sort((a, b) => a.Id.CompareTo(b.Id));
        return infos;
    }

    private List<StageButtonInfo> BuildStageButtonInfos(int chapterId)
    {
        var dict = GameManager.Backend.Chart.Stage.Dictionary;
        int currentStageKey = GameManager.Backend.GameData.UserData.CurrentStageKey;

        var infos = new List<StageButtonInfo>();
        for (int i = 1; i <= 10; i++)
        {
            int stageKey = chapterId * 100 + i;
            if (!dict.TryGetValue(stageKey, out var stageItem))
                continue;

            bool isUnlocked = stageKey <= currentStageKey;
            infos.Add(new StageButtonInfo
            {
                StageKey = stageKey,
                IsUnlocked = isUnlocked
            });
        }
        return infos;
    }

    #region UIEventRegistration
    private void RegisterUIViewEventHandlers()
    {
        lobbyUIView.OnProfileButtonClick += OnProfileButtonClicked;
        lobbyUIView.OnNoticeButtonClick += OnNoticeButtonClicked;
        lobbyUIView.OnShopButtonClick += OnShopButtonClicked;
        lobbyUIView.OnQuestButtonClick += OnQuestButtonClicked;
        lobbyUIView.OnPostButtonClick += OnPostButtonClicked;
        lobbyUIView.OnOptionButtonClick += OnOptionButtonClicked;
        lobbyUIView.OnGemButtonClick += OnGemButtonClicked;
        lobbyUIView.OnGoldButtonClick += OnGoldButtonClicked;
        lobbyUIView.OnHeartButtonClick += OnHeartButtonClicked;
        //lobbyUIView.OnCustomGameButtonClick += OnCustomGameButtonClicked;
        lobbyUIView.OnStageButtonClick += OnStageButtonClicked;
        lobbyUIView.OnChapterButtonClick += OnChapterButtonClicked;

        lobbyPostManager.OnPostChanged += UpdatePostRedDot;
        lobbyQuestManager.OnQuestRedDotChanged += UpdateQuestRedDot;
    }

    private void RegisterUIProfileViewEventHandlers()
    {
        lobbyUIProfileView.OnProfileEditButtonClick += OnProfileEditButtonClicked;
        lobbyUIProfileView.OnProfilePanelExitButtonClick += OnProfilePanelExitButtonClicked;
        lobbyUIProfileView.OnProfileEditConfirmButtonClick += OnProfileEditConfirmButtonClicked;
        lobbyUIProfileView.OnProfileEditCancleButtonClick += OnProfileEditCancleButtonClicked;
    }

    private void RegisterUIOptionViewEventHandlers()
    {
        lobbyUIOptionView.OnContactUsButtonClick += OnContactUsButtonClicked;
        lobbyUIOptionView.OnBackendLoginButtonClick += OnBackendLoginButtonClicked;
        lobbyUIOptionView.OnGoogleLoginButtonClick += OnGoogleLoginButtonClicked;
        lobbyUIOptionView.OnExitButtonClick += OnOptionPanelExitButtonClicked;
        lobbyUIOptionView.OnBGMToggleValueChanged += OnBGMToggleValueChanged;
        lobbyUIOptionView.OnSFXToggleValueChanged += OnSFXToggleValueChanged;
    }
    #endregion

    #region ProfilePanel
    private void OnProfileButtonClicked() => lobbyUIProfileView.SetProfilePanel(true);
    private void OnProfileEditButtonClicked() => lobbyUIProfileView.SetNicknameEditPanel(true);
    private void OnProfilePanelExitButtonClicked() => lobbyUIProfileView.SetProfilePanel(false);
    private void OnProfileEditCancleButtonClicked()
    {
        lobbyUIProfileView.SetNicknameEditPanel(false);
        lobbyUIProfileView.SetNicknameInputFieldReset();
    }
    private void OnProfileEditConfirmButtonClicked()
    {
        string newNickname = lobbyUIProfileView.GetNicknameInputFieldText();

        if (string.IsNullOrWhiteSpace(newNickname))
        {
            GameManager.UI.ShowToast("닉네임을 입력하세요!");
            return;
        }

        GameManager.Backend.ChangeNickname(newNickname, (success, errorMsg) =>
        {
            if (success)
            {
                lobbyUIProfileView.SetNicknameInputFieldReset();
                lobbyUIProfileView.SetNickname(newNickname);
                lobbyUIView.SetNickname(newNickname);
                lobbyUIProfileView.SetNicknameEditPanel(false);
                GameManager.UI.ShowToast("닉네임이 변경되었습니다!");
            }
            else
            {
                GameManager.UI.ShowToast("닉네임 변경 실패!");
            }
        });
    }

    private void UpdateProfileRedDot() => lobbyUIView.SetProfileRedDot(false);
    #endregion

    #region OptionPanel
    private void OnOptionButtonClicked() => lobbyUIOptionView.SetOptionPanel(true);
    private void OnOptionPanelExitButtonClicked() => lobbyUIOptionView.SetOptionPanel(false);
    private void OnContactUsButtonClicked() { }
    private void OnBackendLoginButtonClicked() { }
    private void OnGoogleLoginButtonClicked() { }
    private void OnBGMToggleValueChanged(bool isOn) { }
    private void OnSFXToggleValueChanged(bool isOn) { }
    private void UpdateOptionRedDot() => lobbyUIView.SetOptionRedDot(false);
    #endregion

    #region ShopPanel
    private void OnShopButtonClicked()
    {
        var serverDate = GameManager.Instance.ServerDateTime;
        var userShop = GameManager.Backend.GameData.UserShop;
        bool changed = userShop.CheckShop(serverDate);

        if (changed)
        {
            GameManager.Backend.UpdateAllGameData(null);
        }
        lobbyShopManager.LobbyUIShopView.SetShopPanel(true);
        lobbyShopManager.LobbyUIShopView.SetGoldShopScrollView(true);
        UpdateShopRedDot();
    }
    private void UpdateShopRedDot()
    {
        DateTime serverDate = GameManager.Instance.ServerDateTime;
        bool showDot = !GameManager.Backend.GameData.UserShop.IsShopCheckedToday(serverDate);
        lobbyUIView.SetShopRedDot(showDot);
    }
    #endregion

    #region Quest/Post/CustomGame/Currency
    private void OnQuestButtonClicked() => lobbyQuestManager.QuestMenuButtonClicked();

    private void UpdateQuestRedDot(bool show) => lobbyUIView.SetQuestRedDot(show);
    private void UpdateQuestRedDot()
    {
        int nextId = GameManager.Backend.GameData.QuestAchievement.GetNextIncompleteQuest();

        bool show = false;
        if (nextId != -1)
        {
            bool cleared = GameManager.Backend.GameData.StageAchievement.GetAchieve(nextId);
            bool achieved = GameManager.Backend.GameData.QuestAchievement.Dictionary.TryGetValue(nextId, out var a) && a;
            show = cleared && !achieved;
        }

        UpdateQuestRedDot(show);
    }

    private void OnPostButtonClicked() => lobbyPostManager.PostMenuButtonClicked();
    private void UpdatePostRedDot()
    {
        lobbyUIView.SetPostRedDot(GameManager.Backend.Post.PostList.Count > 0);
    }
    private void OnCustomGameButtonClicked() { }
    private void OnGemButtonClicked()
    {
        lobbyShopManager.LobbyUIShopView.SetShopPanel(true);
        lobbyShopManager.LobbyUIShopView.SetGemShopScrollView(true);
        UpdateShopRedDot();
    }
    private void OnGoldButtonClicked()
    {
        lobbyShopManager.LobbyUIShopView.SetShopPanel(true);
        lobbyShopManager.LobbyUIShopView.SetGoldShopScrollView(true);
        UpdateShopRedDot();
    }
    private void OnHeartButtonClicked()
    {
        lobbyShopManager.LobbyUIShopView.SetShopPanel(true);
        lobbyShopManager.LobbyUIShopView.SetHeartShopScrollView(true);
        UpdateShopRedDot();
    }
    #endregion

    #region Stage & Chapter Buttons
    private void OnStageButtonClicked(int stageKey)
    {
        if (!GameManager.Backend.GameData.UserHeart.TryUseHeart())
        {
            GameManager.UI.ShowToast("하트가 없습니다!");
            return;
        }

        var stageDic = GameManager.Backend.Chart.Stage.Dictionary;

        if (stageDic.TryGetValue(stageKey, out var stageItem))
        {
            string stageName = GameManager.Instance.Language == GameLanguage.ENG
                ? stageItem.Name_ENG
                : stageItem.Name_KR;

            Debug.Log($"[Lobby] Stage 클릭 ▶ {stageName} (Key: {stageKey})");

            GameManager.GameScene.LoadSceneWithStage(stageKey, stageName, SceneNameEnum.BasicGame);
        }
        else
        {
            Debug.LogWarning($"[Lobby] StageKey {stageKey} 가 차트에 없어요!");
        }
    }
    private void OnChapterButtonClicked(int idx)
    {
        var stageInfos = BuildStageButtonInfos(idx);
        lobbyUIView.SetStageButtons(stageInfos);

        int present = GameManager.Backend.GameData.UserData.PresentStageKey;
        lobbyUIView.SetStageScrollViewPosition((present / 100 == idx) ? present : 0);

        lobbyUIView.SetChapterToggle(false);
        ChapterTextUpdate(idx);
    }

    private void ChapterTextUpdate(int idx)
    {
        var chaptername = GameManager.Backend.Chart.Chapter.Dictionary[idx].Name_KR;
        lobbyUIView.SetChapterName(idx, chaptername);
    }
    #endregion

    #region Notice
    private void OnNoticeButtonClicked() => lobbyNoticeManager.LobbyUINoticeView.SetNoticePanel(true);

    private void UpdateNoticeRedDot()
    {
        lobbyUIView.SetNoticeRedDot(false);
    }
    #endregion
}
