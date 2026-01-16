using System;
using System.Collections.Generic;
using System.Reflection;
using BackEnd;
using LitJson;
using UnityEngine;


public class TitleLoadingManager : MonoBehaviour
{
    public static event Action OnLoadingComplete;


    private delegate void BackendLoadStep();
    private readonly Queue<BackendLoadStep> initializeStep = new Queue<BackendLoadStep>();

    public void LoadingStart()
    {
        if (Backend.IsInitialized == false)
        {
            return;
        }
        Init();
        GameManager.Backend.InitInGameData();

        NextStep(true, string.Empty, string.Empty, string.Empty);
    }
    void Init()
    {
        initializeStep.Clear();
        // 트랜잭션으로 불러온 후, 안불러질 경우 각자 Get 함수로 불러오는 함수
        initializeStep.Enqueue(() => { TransactionRead(NextStep); });

        // 차트정보 불러오기 함수 Insert
        initializeStep.Enqueue(() => { GameManager.Backend.Chart.ChartInfo.BackendLoad(NextStep); });
        initializeStep.Enqueue(() => { GameManager.Backend.Chart.Stage.BackendChartDataLoad(NextStep); });
        initializeStep.Enqueue(() => { GameManager.Backend.Chart.Chapter.BackendChartDataLoad(NextStep); });
        initializeStep.Enqueue(() => { GameManager.Backend.Chart.Quests.BackendChartDataLoad(NextStep); });
        initializeStep.Enqueue(() => { GameManager.Backend.Chart.QuestReward.BackendChartDataLoad(NextStep); });
        initializeStep.Enqueue(() => { GameManager.Backend.Chart.ShopDefaultProduct.BackendChartDataLoad(NextStep); });
        initializeStep.Enqueue(() => { GameManager.Backend.Chart.ShopDefaultPrice.BackendChartDataLoad(NextStep); });
        initializeStep.Enqueue(() => { GameManager.Backend.Chart.ShopDefaultGrant.BackendChartDataLoad(NextStep); });
        initializeStep.Enqueue(() => { GameManager.Backend.Chart.Item.BackendChartDataLoad(NextStep); });
        
        initializeStep.Enqueue(() => { GameManager.Backend.Notice.BackendLoad(NextStep); });
        initializeStep.Enqueue(() => { GameManager.Backend.Post.BackendLoad(NextStep); });
    }

    private void NextStep(bool isSuccess, string className, string funcName, string errorInfo)
    {
        if (isSuccess)
        {
            if (initializeStep.Count > 0)
            {
                initializeStep.Dequeue().Invoke();
            }
            else
            {
                TouchToStartGo();
            }
        }
        else
        { 
            GameManager.UI.ShowToast($"{className}, {funcName}, {errorInfo}");
        }
    }

    private void TransactionRead(BackendData.Base.Normal.AfterBackendLoadFunc func)
    {
        bool isSuccess = false;
        string className = GetType().Name;
        string functionName = MethodBase.GetCurrentMethod()?.Name;
        string errorInfo = string.Empty;

        List<TransactionValue> transactionList = new List<TransactionValue>();

        foreach (var gameData in GameManager.Backend.GameData.GameDataList)
        {
            transactionList.Add(gameData.Value.GetTransactionGetValue());
        }

        SendQueue.Enqueue(Backend.GameData.TransactionReadV2, transactionList, callback => {
            try
            {
                Debug.Log($"Backend.GameData.TransactionReadV2 : {callback}");

                if (callback.IsSuccess())
                {
                    JsonData gameDataJson = callback.GetFlattenJSON()["Responses"];
                    int index = 0;

                    foreach (var gameData in GameManager.Backend.GameData.GameDataList)
                    {
                        initializeStep.Enqueue(() => {
                            gameData.Value.BackendGameDataLoadByTransaction(gameDataJson[index++], NextStep);
                        });
                    }
                    isSuccess = true;
                }
                else
                {
                    foreach (var gameData in GameManager.Backend.GameData.GameDataList)
                    {
                        initializeStep.Enqueue(() => {
                            gameData.Value.BackendGameDataLoad(NextStep);
                        });
                    }
                    isSuccess = true;
                }
            }
            catch (Exception e)
            {
                errorInfo = e.ToString();
            }
            finally
            {
                func.Invoke(isSuccess, className, functionName, errorInfo);
            }
        });
    }


    private void TouchToStartGo()
    {
        initializeStep.Clear();
        OnLoadingComplete?.Invoke();
    }
}
