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
        Debug.Log("�α��� �õ� ����...");
        SendQueue.Enqueue(Backend.BMember.LoginWithTheBackendToken, callback => {
            Debug.Log($"Backend.BMember.LoginWithTheBackendToken : {callback}");

            if (callback.IsSuccess())
            {
                Debug.Log($"��ū �α��� ����: {Backend.UserNickName}");
                GoToNext();
            }
            else
            {
                Debug.LogWarning($"��ū �α��� ����: {callback} �Խ�Ʈ �α����� �õ��մϴ�.");
                AttemptGuestLogin();
            }
        });
    }

    private void AttemptGuestLogin()
    {
        Debug.Log("�Խ�Ʈ �α��� �õ�...");
        SendQueue.Enqueue(Backend.BMember.GuestLogin, callback => {
            Debug.Log($"Backend.BMember.GuestLogin : {callback}");

            if (callback.IsSuccess())
            {
                Debug.Log($"�Խ�Ʈ �α��� ����: {Backend.UserNickName} (InDate: {Backend.UserInDate})");
                GoToNext();
            }
            else
            {
                Debug.LogError($"�Խ�Ʈ �α��� ����: {callback}");
            }
        });
    }

    private void GoToNext()
    {
        titleLoadingManager.LoadingStart();
    }
}
