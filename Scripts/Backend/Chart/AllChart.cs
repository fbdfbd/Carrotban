// Copyright 2013-2022 AFI, INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;
using BackEnd;
using BackendData.Base;
using LitJson;
using Unity.VisualScripting;
using UnityEngine;

namespace BackendData.Chart
{
    public class AllChart : Normal
    {

        // ��Ʈ�� ���� id�� �����ϴ� Dictionary
        private readonly Dictionary<string, string> _chartDictionary = new();
        // �ٸ� Ŭ�������� Add, Delete�� ������ �Ұ����ϵ��� �б� ���� Dictionary
        public IReadOnlyDictionary<string, string> Dictionary => (IReadOnlyDictionary<string, string>)_chartDictionary.AsReadOnlyCollection();

        // �������� �����͸� �ҷ��� �Ľ��ϴ� �Լ�
        public override void BackendLoad(AfterBackendLoadFunc afterBackendLoadFunc)
        {

            bool isSuccess = false;
            string errorInfo = string.Empty;
            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            // [�ڳ�] ��Ʈ ���� �ҷ����� �Լ�
            SendQueue.Enqueue(Backend.Chart.GetChartList, callback => {
                try
                {
                    Debug.Log($"Backend.Chart.GetChartList : {callback}");

                    if (!callback.IsSuccess())
                    {
                        throw new Exception(callback.ToString());
                    }

                    JsonData json = callback.FlattenRows();

                    for (int i = 0; i < json.Count; i++)
                    {
                        string chartName = json[i]["chartName"].ToString(); // ��Ʈ �̸� �Ľ�
                        string selectedChartFileId = json[i]["selectedChartFileId"].ToString(); // ��Ʈ ���� ���̵� �Ľ�

                        if (_chartDictionary.ContainsKey(chartName))
                        {
                            Debug.LogWarning($"������ ��Ʈ Ű ���� �����մϴ� : {chartName} - {selectedChartFileId}"); // ������̽�
                        }
                        else
                        {
                            _chartDictionary.Add(chartName, selectedChartFileId);
                        }
                    }

                    isSuccess = true;
                }
                catch (Exception e)
                {
                    errorInfo = e.Message;
                }
                finally
                {
                    afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);
                }
            });
        }
    }
}