using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BackEnd;
using static LobbyManager;

public class LobbyUIView : MonoBehaviour
{
    [Header("텍스트")]
    [SerializeField] private TMP_Text gemText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text heartText;
    [SerializeField] private TMP_Text chapterText;
    [SerializeField] private TMP_Text custommodText;
    [SerializeField] private TMP_Text nicknameText;

    [Header("빨간점")]
    [SerializeField] private GameObject noticeRedDot;
    [SerializeField] private GameObject shopRedDot;
    [SerializeField] private GameObject questRedDot;
    [SerializeField] private GameObject postRedDot;
    [SerializeField] private GameObject optionRedDot;
    [SerializeField] private GameObject profileRedDot;

    [Header("상단 버튼")]
    [SerializeField] private Button profileButton;
    [SerializeField] private Button noticeButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button questButton;
    [SerializeField] private Button postButton;
    [SerializeField] private Button optionButton;

    [Header("중단 버튼")]
    [SerializeField] private Button gemButton;
    [SerializeField] private Button goldButton;
    [SerializeField] private Button heartButton;

    [Header("스테이지 버튼")]
    [SerializeField] private Toggle chapterToggle;
    [SerializeField] private List<Button> stageButtons;
    [SerializeField] private List<Button> chapterButtons;

    //[Header("하단 버튼")]
    //[SerializeField] private Button customGameButton;

    [Header("상단 버튼")]
    [SerializeField] private ScrollRect stageScrollRect;

    [Header("하트")]
    [SerializeField] private GameObject heartTimer;

    public event Action OnProfileButtonClick;
    public event Action OnNoticeButtonClick;
    public event Action OnShopButtonClick;
    public event Action OnQuestButtonClick;
    public event Action OnPostButtonClick;
    public event Action OnOptionButtonClick;
    public event Action OnGemButtonClick;
    public event Action OnGoldButtonClick;
    public event Action OnHeartButtonClick;
    //public event Action OnCustomGameButtonClick;

    public event Action<int> OnStageButtonClick;
    public event Action<int> OnChapterButtonClick;

    public void Init()
    {
        profileButton.onClick.RemoveAllListeners();
        profileButton.onClick.AddListener(() => OnProfileButtonClick?.Invoke());

        noticeButton.onClick.RemoveAllListeners();
        noticeButton.onClick.AddListener(() => OnNoticeButtonClick?.Invoke());

        shopButton.onClick.RemoveAllListeners();
        shopButton.onClick.AddListener(() => OnShopButtonClick?.Invoke());

        questButton.onClick.RemoveAllListeners();
        questButton.onClick.AddListener(() => OnQuestButtonClick?.Invoke());

        postButton.onClick.RemoveAllListeners();
        postButton.onClick.AddListener(() => OnPostButtonClick?.Invoke());

        optionButton.onClick.RemoveAllListeners();
        optionButton.onClick.AddListener(() => OnOptionButtonClick?.Invoke());

        gemButton.onClick.RemoveAllListeners();
        gemButton.onClick.AddListener(() => OnGemButtonClick?.Invoke());

        goldButton.onClick.RemoveAllListeners();
        goldButton.onClick.AddListener(() => OnGoldButtonClick?.Invoke());

        heartButton.onClick.RemoveAllListeners();
        heartButton.onClick.AddListener(() => OnHeartButtonClick?.Invoke());

        //customGameButton.onClick.RemoveAllListeners();
        //customGameButton.onClick.AddListener(() => OnCustomGameButtonClick?.Invoke());
    }

    public void SetHeartTimer(bool active)
    {
        heartTimer.SetActive(active);
    }

    public HeartTimer GetHeartTimer()
    {
        return heartTimer.GetComponent<HeartTimer>();
    }

    public void SetStageScrollViewPosition(int stage)
    {
        if(stage == 0)
        {
            stageScrollRect.verticalNormalizedPosition = 0f;
            return;
        }
        int stageNum = stage % 100;             
        stageNum = Mathf.Clamp(stageNum, 1, 10); 

        float normalized = (stageNum - 1) / 9f; 

        stageScrollRect.verticalNormalizedPosition = normalized;
    }

    public void SetChapterToggle(bool active)
    {
        chapterToggle.isOn = active;
    }

    public void SetNoticeRedDot(bool show)
    {
        noticeRedDot.SetActive(show);
    }
    public void SetShopRedDot(bool show)
    {
        shopRedDot.SetActive(show);
    }
    public void SetQuestRedDot(bool show)
    {
        questRedDot.SetActive(show);
    }
    public void SetPostRedDot(bool show)
    {
        postRedDot.SetActive(show);
    }

    public void SetOptionRedDot(bool show)
    {
        optionRedDot.SetActive(show);
    }

    public void SetProfileRedDot(bool show)
    {
        profileRedDot.SetActive(show);
    }

    public void SetChapterName(int num, string name)
    {
        chapterText.text = $"{num}. {name}";
    }

    public void SetGold(int amount)
    {
        goldText.text = amount.ToString("0");
    }

    public void SetGem(int amount)
    {
        gemText.text = amount.ToString("0");
    }

    public void SetHeart(int amount)
    {
        heartText.text = amount.ToString();
    }

    public void SetNickname(string name)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
        {
            name = "GUEST";
        }

        nicknameText.text = name;
    }

    public void SetChapterButtons(List<ChapterButtonInfo> chapterInfos)
    {
        for (int i = 0; i < chapterButtons.Count; i++)
        {
            if (i < chapterInfos.Count && chapterInfos[i].IsUnlocked)
            {
                chapterButtons[i].gameObject.SetActive(true);

                string chapterName = GameManager.Instance.Language == GameLanguage.ENG
                    ? chapterInfos[i].Name_ENG
                    : chapterInfos[i].Name_KR;

                chapterButtons[i].GetComponentInChildren<TMP_Text>().text =
                    $"{chapterInfos[i].Id}. {chapterName}";

                chapterButtons[i].onClick.RemoveAllListeners();
                int chapterId = chapterInfos[i].Id;
                chapterButtons[i].onClick.AddListener(() => OnChapterButtonClick?.Invoke(chapterId));
            }
            else
            {
                chapterButtons[i].gameObject.SetActive(false);
            }
        }
    }
    public void SetStageButtons(List<StageButtonInfo> stageInfos)
    {
        for (int i = 0; i < stageButtons.Count; i++)
        {
            if (i < stageInfos.Count && stageInfos[i].IsUnlocked)
            {
                stageButtons[i].gameObject.SetActive(true);
                int stageNum = stageInfos[i].StageKey % 100;
                stageButtons[i].GetComponentInChildren<TMPro.TMP_Text>().text = $"{stageNum}";
                stageButtons[i].transform.GetChild(1).gameObject.SetActive(false);

                stageButtons[i].onClick.RemoveAllListeners();
                int stageKey = stageInfos[i].StageKey;

                stageButtons[i].onClick.AddListener(() => OnStageButtonClick?.Invoke(stageKey));
            }
            else
            {
                stageButtons[i].onClick.RemoveAllListeners();
                stageButtons[i].transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }
}
