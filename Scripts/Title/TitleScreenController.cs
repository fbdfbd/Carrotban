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
        Debug.Log("��ġ���۰���");
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
                // UI �� Ŭ���� ����
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
                    // UI �� ��ġ�� ����
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
        Debug.Log($"���� �� �ε� ��û: {nextSceneToLoad.ToString()}");
        if (GameManager.GameScene != null)
        {
            GameManager.GameScene.LoadSceneWithFade(nextSceneToLoad);
        }
        else
        {
            Debug.LogError("GameSceneManager �ν��Ͻ��� ã�� �� �����ϴ�! ���� ���� �ε��� �� �����ϴ�.");
            sceneLoadInitiated = false;
        }
    }
}