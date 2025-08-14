using BackEnd;
using UnityEngine;

public class BackendLoginManager : MonoBehaviour
{
    [SerializeField]
    private TitleLoadingManager titleLoadingManager;

    private void Start()
    {
        AttemptLogin();
    }

    public void AttemptLogin()
    {
        Debug.Log("로그인 시도 시작...");
        SendQueue.Enqueue(Backend.BMember.LoginWithTheBackendToken, callback => {
            Debug.Log($"Backend.BMember.LoginWithTheBackendToken : {callback}");

            if (callback.IsSuccess())
            {
                Debug.Log($"토큰 로그인 성공: {Backend.UserNickName}");
                GoToNext();
            }
            else
            {
                Debug.LogWarning($"토큰 로그인 실패: {callback} 게스트 로그인을 시도합니다.");
                AttemptGuestLogin();
            }
        });
    }

    private void AttemptGuestLogin()
    {
        Debug.Log("게스트 로그인 시도...");
        SendQueue.Enqueue(Backend.BMember.GuestLogin, callback => {
            Debug.Log($"Backend.BMember.GuestLogin : {callback}");

            if (callback.IsSuccess())
            {
                Debug.Log($"게스트 로그인 성공: {Backend.UserNickName} (InDate: {Backend.UserInDate})");
                GoToNext();
            }
            else
            {
                Debug.LogError($"게스트 로그인 실패: {callback}");
            }
        });
    }

    private void GoToNext()
    {
        titleLoadingManager.LoadingStart();
    }
}
