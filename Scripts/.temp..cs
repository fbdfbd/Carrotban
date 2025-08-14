using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageUIView : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Player player; // 게임 데이터 접근용

    [Header("UI Elements")]
    // [SerializeField] private TextMeshProUGUI seedStatusText; // 씨앗 상태 텍스트
    [SerializeField] private Image seedIcon; // 씨앗 아이콘 이미지
    [SerializeField] private Button actionButton; // 액션 버튼 (StageManager에서 연결했지만, 여기서도 참조 가능)
    [SerializeField] private Button undoButton;
    [SerializeField] private Button redoButton;
    // ... 기타 필요한 UI 요소들

    [Header("Managers")]
    private PlayerController playerController; // 직접 호출 위해
    private UndoManager undoManager;

    private void Start()
    {
        // 필요한 참조 설정
        if (player == null) player = FindObjectOfType<Player>(); // 씬에서 찾기
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            // 이벤트 구독 (데이터 변경 시 UI 업데이트)
            player.OnHasSeedChanged += UpdateSeedUI;
            // 초기 UI 업데이트
            UpdateSeedUI(player.HasSeed);
        }
        else Debug.LogError("Player not found for StageUIView!");

        undoManager = UndoManager.Instance; // 싱글톤 인스턴스 가져오기

        // 버튼 리스너 설정 (StageManager와 중복될 수 있으므로 역할 분담 필요)
        // StageManager가 버튼 입력을 받고 Controller 호출 -> Controller가 게임 로직 처리
        // StageUIView는 게임 상태를 받아서 UI를 업데이트하는 역할에 집중하는 것이 좋음.
        // 따라서 StageUIView에서 버튼 리스너를 직접 설정하기보다, StageManager의 역할을 유지하는 것을 추천.
        // 만약 이 View가 자체적인 버튼 로직(예: 설정 열기)을 갖는다면 여기서 설정.
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (player != null)
        {
            player.OnHasSeedChanged -= UpdateSeedUI;
        }
    }

    /// <summary>
    /// 씨앗 보유 상태에 따라 UI를 업데이트합니다.
    /// </summary>
    private void UpdateSeedUI(bool hasSeed)
    {
        // if (seedStatusText != null) seedStatusText.text = hasSeed ? "씨앗 보유" : "씨앗 없음";
        if (seedIcon != null) seedIcon.enabled = hasSeed; // 아이콘 활성/비활성화
    }

    // 예시: 게임 클리어 시 호출되어 버튼 비활성화
    public void DisableInteraction()
    {
        //actionButton?.interactable = false;
        //undoButton?.interactable = false;
        //redoButton?.interactable = false;
        // 이동 버튼도 비활성화 필요 시 참조 추가
    }
}