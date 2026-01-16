using UnityEngine;
using UnityEngine.EventSystems;

public class TitleScreenController : MonoBehaviour
{
    public SceneNameEnum nextSceneToLoad = SceneNameEnum.Lobby;

    private bool sceneLoadInitiated = false;
    private bool canStartGameByTouch = false;

    void Start()
    {
        TitleLoadingManager.OnLoadingComplete += HandleLoadingComplete;
    }

    void OnDestroy()
    {
        TitleLoadingManager.OnLoadingComplete -= HandleLoadingComplete;
    }

    private void HandleLoadingComplete()
    {
        Debug.Log("터치시작가능");
        canStartGameByTouch = true;
    }

    void Update()
    {
        if (sceneLoadInitiated)
        {
            return;
        }

        if (!canStartGameByTouch)
        {
            return;
        }

        if (Input.anyKeyDown)
        {
            StartGameSequence();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                //무시
            }
            else
            {
                StartGameSequence();
                return;
            }
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    //무시
                }
                else
                {
                    StartGameSequence();
                    return;
                }
            }
        }
    }

    void StartGameSequence()
    {
        if (sceneLoadInitiated) return;
        sceneLoadInitiated = true;
        Debug.Log($"다음 씬 로드 요청: {nextSceneToLoad.ToString()}");
        if (GameManager.GameScene != null)
        {
            GameManager.GameScene.LoadSceneWithFade(nextSceneToLoad);
        }
        else
        {
            Debug.LogError("GameSceneManager 인스턴스를 찾을 수 없습니다! 다음 씬을 로드할 수 없습니다.");
            sceneLoadInitiated = false;
        }
    }
}