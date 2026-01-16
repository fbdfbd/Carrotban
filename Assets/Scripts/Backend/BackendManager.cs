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

    private bool _isErrorOccured = false; // 치명적인 에러 발생 여부 

    public void Init()
    {
        var initializeBro = Backend.Initialize();

        if (initializeBro.IsSuccess())
        {
            Debug.Log("뒤끝 초기화가 완료되었습니다.");
            CreateSendQueueMgr();
            SetErrorHandler();
        }
        else
        {
            Debug.Log("뒤끝 초기화가 실패했습니다.");
        }

        string hash = Backend.Utils.GetGoogleHash();
        Debug.Log($"<color=green>Google Hash:</color> <color=yellow>{hash}</color>");
    }

    private void SetErrorHandler()
    {
        Backend.ErrorHandler.OnMaintenanceError = () => {
            Debug.LogError("서버점검중");
        };
        Backend.ErrorHandler.OnTooManyRequestError = () => {
            Debug.LogError("403비정상감지");
        };
        Backend.ErrorHandler.OnOtherDeviceLoginDetectedError = () => {
            Debug.LogError("다른기기로그인감지");
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


    public void StartUpdate()
    {
        StartCoroutine(UpdateGameDataTransaction());
        StartCoroutine(GetAdminPostList());
    }

    public void StopUpdate()
    {
        Debug.Log("자동 저장을 중지합니다.");
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

    // 업데이트가 발생한 이후에 호출에 대한 응답을 반환해주는 대리자 함수
    public delegate void AfterUpdateFunc(BackendReturnObject callback);


    // 값이 바뀐 데이터가 있는지 체크후 바뀐 데이터들은 바로 저장 혹은 트랜잭션에 묶어 저장을 진행하는 함수
    public void UpdateAllGameData(AfterUpdateFunc afterUpdateFunc)
    {
        string info = string.Empty;


        // 바뀐 데이터가 몇개 있는지 체크
        
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
                Debug.Log("저장할 데이터가 없습니다");
                return;
            }
            afterUpdateFunc(null); // 지정한 대리자 함수 호출

            // 업데이트할 목록이 존재하지 않습니다.
        }
        else if (gameDatas.Count == 1)
        {

            //하나라면 찾아서 해당 테이블만 업데이트
            foreach (var gameData in gameDatas)
            {
                if (gameData.IsChangedData)
                {
                    gameData.Update(callback => {

                        //성공할경우 데이터 변경 여부를 false로 변경
                        if (callback.IsSuccess())
                        {
                            gameData.IsChangedData = false;
                        }
                        else
                        {
                            SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), callback.ToString() + "\n" + info);
                        }
                        Debug.Log($"UpdateV2 : {callback}\n업데이트 테이블 : \n{info}");
                        if (afterUpdateFunc == null)
                        {

                        }
                        else
                        {
                            afterUpdateFunc(callback); // 지정한 대리자 함수 호출
                        }
                    });
                }
            }
        }
        else
        {
            // 2개 이상이라면 트랜잭션에 묶어서 업데이트
            // 단 10개 이상이면 트랜잭션 실패 주의
            List<TransactionValue> transactionList = new List<TransactionValue>();

            // 변경된 데이터만큼 트랜잭션 추가
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

                Debug.Log($"TransactionWriteV2 : {callback}\n업데이트 테이블 : \n{info}");

                if (afterUpdateFunc == null)
                {

                }
                else
                {

                    afterUpdateFunc(callback);  // 지정한 대리자 함수 호출
                }
            });
        }
        
    }

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
                            GameManager.UI.ShowToast("우편이 도착했습니다!");
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


    public void SendBugReport(string className, string functionName, string errorInfo, int repeatCount = 3)
    {
        if (repeatCount <= 0)
        {
            return;
        }

        if (string.IsNullOrEmpty(Backend.UserInDate))
        {
            return;
        }

        Param param = new Param();
        param.Add("className", className);
        param.Add("functionName", functionName);
        param.Add("errorPath", errorInfo);

        // [뒤끝] 로그 삽입 함수
        Backend.GameLog.InsertLogV2("error", param, 7, callback => {
            // 에러가 발생할 경우 재귀
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