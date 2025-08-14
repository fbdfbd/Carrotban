using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameSceneManager : MonoBehaviour
{
    [Tooltip("ȭ�� ��ȯ(���̵� ��/�ƿ�)�� �ɸ��� �ð�")]
    public float fadeDuration = 0.5f;

    public float CurrentLoadingProgress { get; private set; }
    private string stageName;
    private string _targetSceneNameString;
    public string StageName => stageName;

    public void LoadSceneWithStage(int num, string name, SceneNameEnum sceneToLoad)
    {
        _targetSceneNameString = sceneToLoad.ToString();
        if (string.IsNullOrEmpty(_targetSceneNameString))
        {
            Debug.LogError("�ε��� �� �̸� ��ȯ�� �����߽��ϴ�!");
            return;
        }

        stageName = name;

        GameManager.Backend.GameData.UserData.UpdatePresentStage(num);
        GameManager.Backend.UpdateAllGameData(null);

        StartCoroutine(LoadSceneRoutine(false));
    }

    public void LoadSceneWithFade(SceneNameEnum sceneToLoad)
    {
        _targetSceneNameString = sceneToLoad.ToString();
        if (string.IsNullOrEmpty(_targetSceneNameString))
        {
            Debug.LogError("�ε��� �� �̸� ��ȯ�� �����߽��ϴ�!");
            return;
        }
        StartCoroutine(LoadSceneRoutine(true));
    }

    public void LoadSceneImmediate(SceneNameEnum sceneToLoad)
    {
        _targetSceneNameString = sceneToLoad.ToString();
        if (string.IsNullOrEmpty(_targetSceneNameString))
        {
            Debug.LogError("�ε��� �� �̸� ��ȯ�� �����߽��ϴ�!");
            return;
        }
        StartCoroutine(LoadSceneRoutine(false));
    }

    public void ReloadCurrentScene()
    {
        string currentSceneNameStr = SceneManager.GetActiveScene().name;
        if (Enum.TryParse<SceneNameEnum>(currentSceneNameStr, out SceneNameEnum currentSceneEnum))
        {
            LoadSceneWithFade(currentSceneEnum);
        }
        else
        {
            Debug.LogError($"���� �� '{currentSceneNameStr}'�� SceneName enum�� �����ϴ�.");
        }
    }

    public void QuitGame()
    {
        Debug.Log("������ �����մϴ�...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator LoadSceneRoutine(bool applyFadeEffect)
    {
        if (applyFadeEffect && GameManager.UI != null)
        {
            GameManager.UI.FadeOut(fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }

        GameManager.UI?.ShowLoadingScreen(_targetSceneNameString);

        CurrentLoadingProgress = 0f;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_targetSceneNameString);

        while (!asyncLoad.isDone)
        {
            CurrentLoadingProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            GameManager.UI?.UpdateLoadingProgress(CurrentLoadingProgress);
            yield return null;
        }

        CurrentLoadingProgress = 1f;
        GameManager.UI?.UpdateLoadingProgress(CurrentLoadingProgress);

        yield return null;
        GameManager.UI?.HideLoadingScreen();

        PlaySceneBGM(_targetSceneNameString);

        if (applyFadeEffect)
        {
            GameManager.UI?.FadeIn(fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }
    }


    private void PlaySceneBGM(string loadedSceneName)
    {
        var sm = GameManager.SoundManager;
        if (sm == null) return;

        // Title/Lobby
        if (loadedSceneName.Equals(SceneNameEnum.Title.ToString(), StringComparison.OrdinalIgnoreCase) ||
            loadedSceneName.Equals(SceneNameEnum.Lobby.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            sm.PlayTitleOrLobbyBGM();
            return;
        }

        // BasicGame
        if (loadedSceneName.Equals(SceneNameEnum.BasicGame.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            int stageKey = GameManager.Backend.GameData.UserData.PresentStageKey; // e.g., 201, 103
            int chapterId = Mathf.Max(1, stageKey / 100); // 201��2, 103��1
            sm.PlayStageBGMByChapter(chapterId);
            return;
        }

        sm.PlayTitleOrLobbyBGM();
    }

}
