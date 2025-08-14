using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BackEnd;
using BackendData.Base;
using UnityEngine;


public class BackendManager : MonoBehaviour
{
    public class BackendChart
    {
        public readonly BackendData.Chart.AllChart ChartInfo = new();
        public readonly BackendData.Chart.Stage.Manager Stage = new();
        public readonly BackendData.Chart.Chapter.Manager Chapter = new();
        public readonly BackendData.Chart.Quests.Manager Quests = new();
        public readonly BackendData.Chart.QuestReward.Manager QuestReward = new();
        public readonly BackendData.Chart.ShopDefaultProduct.Manager ShopDefaultProduct = new();
        public readonly BackendData.Chart.ShopDefaultPrice.Manager ShopDefaultPrice = new();
        public readonly BackendData.Chart.ShopDefaultGrant.Manager ShopDefaultGrant = new();
        public readonly BackendData.Chart.Item.Manager Item = new();
    }

    public class BackendGameData
    {
        public readonly BackendData.GameData.UserData UserData = new();
        public readonly BackendData.GameData.UserHeart UserHeart = new();
        public readonly BackendData.GameData.UserGem UserGem = new();
        public readonly BackendData.GameData.UserShop UserShop = new();
        public readonly BackendData.GameData.QuestAchievement.Manager QuestAchievement = new();
        public readonly BackendData.GameData.StageAchievement.Manager StageAchievement = new();
        public readonly BackendData.GameData.UserInventory.Manager UserInventory = new();

        public readonly Dictionary<string, BackendData.Base.GameData>
            GameDataList = new Dictionary<string, GameData>();

        public BackendGameData()
        {
            GameDataList.Add("userdata", UserData);
            GameDataList.Add("userheart", UserHeart);
            GameDataList.Add("usergem", UserGem);
            GameDataList.Add("usershop", UserShop);
            GameDataList.Add("userquest", QuestAchievement);
            GameDataList.Add("userstage", StageAchievement);
            GameDataList.Add("userinventory", UserInventory);
        }
    }

    public BackendChart Chart = new();
    public BackendGameData GameData = new();
    public BackendData.Post.Manager Post = new();
    public BackendData.Notice.Manager Notice = new();

    private bool _isErrorOccured = false; // ġ������ ���� �߻� ���� 

    public void Init()
    {
        var initializeBro = Backend.Initialize();

        if (initializeBro.IsSuccess())
        {
            Debug.Log("�ڳ� �ʱ�ȭ�� �Ϸ�Ǿ����ϴ�.");
            CreateSendQueueMgr();
            SetErrorHandler();
        }
        else
        {
            Debug.Log("�ڳ� �ʱ�ȭ�� �����߽��ϴ�.");
        }
    }

    private void SetErrorHandler()
    {
        Backend.ErrorHandler.OnMaintenanceError = () => {
            Debug.LogError("����������");
        };
        Backend.ErrorHandler.OnTooManyRequestError = () => {
            Debug.LogError("403��������");
        };
        Backend.ErrorHandler.OnOtherDeviceLoginDetectedError = () => {
            Debug.LogError("�ٸ����α��ΰ���");
        };
    }

    public void InitInGameData()
    {
        Chart = new();
        GameData = new();
        Notice = new();
        Post = new();
    }

    private void CreateSendQueueMgr()
    {
        var obj = new GameObject("SendQueueMgr");
        obj.AddComponent<SendQueueMgr>();
        DontDestroyOnLoad(obj);
    }


    //�����ֱ⸶�� ������Ʈ..�ε�?
    public void StartUpdate()
    {
        StartCoroutine(UpdateGameDataTransaction());
        StartCoroutine(GetAdminPostList());
    }

    public void StopUpdate()
    {
        Debug.Log("�ڵ� ������ �����մϴ�.");
        _isErrorOccured = false;
    }
    private IEnumerator UpdateGameDataTransaction()
    {
        var seconds = new WaitForSeconds(300);
        yield return seconds;

        
        while (_isErrorOccured)
        {
            UpdateAllGameData(null);

            yield return seconds;
        }
        
    }

    // ������Ʈ�� �߻��� ���Ŀ� ȣ�⿡ ���� ������ ��ȯ���ִ� �븮�� �Լ�
    public delegate void AfterUpdateFunc(BackendReturnObject callback);


    // ���� �ٲ� �����Ͱ� �ִ��� üũ�� �ٲ� �����͵��� �ٷ� ���� Ȥ�� Ʈ����ǿ� ���� ������ �����ϴ� �Լ�
    public void UpdateAllGameData(AfterUpdateFunc afterUpdateFunc)
    {
        string info = string.Empty;


        // �ٲ� �����Ͱ� � �ִ��� üũ
        
        List<GameData> gameDatas = new List<GameData>();

        foreach (var gameData in GameData.GameDataList)
        {
            if (gameData.Value.IsChangedData)
            {
                info += gameData.Value.GetTableName() + "\n";
                gameDatas.Add(gameData.Value);
            }
        }

        if (gameDatas.Count <= 0)
        {
            if (afterUpdateFunc == null)
            {
                Debug.Log("������ �����Ͱ� �����ϴ�");
                return;
            }
            afterUpdateFunc(null); // ������ �븮�� �Լ� ȣ��

            // ������Ʈ�� ����� �������� �ʽ��ϴ�.
        }
        else if (gameDatas.Count == 1)
        {

            //�ϳ���� ã�Ƽ� �ش� ���̺� ������Ʈ
            foreach (var gameData in gameDatas)
            {
                if (gameData.IsChangedData)
                {
                    gameData.Update(callback => {

                        //�����Ұ�� ������ ���� ���θ� false�� ����
                        if (callback.IsSuccess())
                        {
                            gameData.IsChangedData = false;
                        }
                        else
                        {
                            SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), callback.ToString() + "\n" + info);
                        }
                        Debug.Log($"UpdateV2 : {callback}\n������Ʈ ���̺� : \n{info}");
                        if (afterUpdateFunc == null)
                        {

                        }
                        else
                        {
                            afterUpdateFunc(callback); // ������ �븮�� �Լ� ȣ��
                        }
                    });
                }
            }
        }
        else
        {
            // 2�� �̻��̶�� Ʈ����ǿ� ��� ������Ʈ
            // �� 10�� �̻��̸� Ʈ����� ���� ����
            List<TransactionValue> transactionList = new List<TransactionValue>();

            // ����� �����͸�ŭ Ʈ����� �߰�
            foreach (var gameData in gameDatas)
            {
                transactionList.Add(gameData.GetTransactionUpdateValue());
            }

            SendQueue.Enqueue(Backend.GameData.TransactionWriteV2, transactionList, callback => {
                Debug.Log($"Backend.BMember.TransactionWriteV2 : {callback}");

                if (callback.IsSuccess())
                {
                    foreach (var data in gameDatas)
                    {
                        data.IsChangedData = false;
                    }
                }
                else
                {
                    //SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), callback.ToString() + "\n" + info);
                }

                Debug.Log($"TransactionWriteV2 : {callback}\n������Ʈ ���̺� : \n{info}");

                if (afterUpdateFunc == null)
                {

                }
                else
                {

                    afterUpdateFunc(callback);  // ������ �븮�� �Լ� ȣ��
                }
            });
        }
        
    }


    // ���� �ֱ⸶�� ������ �ҷ����� �ڷ�ƾ �Լ�
    private IEnumerator GetAdminPostList()
    {
        var seconds = new WaitForSeconds(600);

        yield return seconds;

        while (_isErrorOccured)
        {
            int postCount = (Post != null && Post.PostList != null) ? Post.PostList.Count : 0;

            Post.GetPostList(PostType.Admin, (success, info) => {
                if (success)
                {
                    int currentCount = (Post != null && Post.PostList != null) ? Post.PostList.Count : 0;
                    if (postCount != currentCount)
                    {
                        if (currentCount > 0)
                        {
                            GameManager.UI.ShowToast("������ �����߽��ϴ�!");
                        }
                    }
                }
                else
                {
                    SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), info);
                }
            });

            yield return seconds;
        }
    }




    // ���� �߻��� ���ӷα׸� �����ϴ� �Լ�
    public void SendBugReport(string className, string functionName, string errorInfo, int repeatCount = 3)
    {
        if (repeatCount <= 0)
        {
            return;
        }

        // �α��ξȵ�
        if (string.IsNullOrEmpty(Backend.UserInDate))
        {
            return;
        }

        Param param = new Param();
        param.Add("className", className);
        param.Add("functionName", functionName);
        param.Add("errorPath", errorInfo);

        // [�ڳ�] �α� ���� �Լ�
        Backend.GameLog.InsertLogV2("error", param, 7, callback => {
            // ������ �߻��� ��� ���
            if (callback.IsSuccess() == false)
            {
                SendBugReport(className, functionName, errorInfo, repeatCount - 1);
            }
        });
    }
    public void ChangeNickname(string nickname, Action<bool, string> onComplete)
    {
        SendQueue.Enqueue(Backend.BMember.UpdateNickname, nickname, callback =>
        {
            bool success = callback.IsSuccess();
            string errorMsg = callback.GetMessage();
            onComplete?.Invoke(success, errorMsg);
        });
    }
}