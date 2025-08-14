// Copyright 2013-2022 AFI, INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;
using BackEnd;
using LitJson;
using UnityEngine;

namespace BackendData.Base
{
    //===============================================================
    // ��Ʈ �ҷ����⿡ ���� �������� ������ ���� Ŭ����
    //===============================================================
    public abstract class Chart : Normal
    {

        // Backend.Chart.GetChartContents ȣ�� �� ���ϵǴ� json(Flatten)�� ó���ϴ� �Լ�
        // �� �ڽ� ��ü���� ���¿� �°� �����ؾ��Ѵ�.
        protected abstract void LoadChartDataTemplate(JsonData json);
        public abstract string GetChartFileName(); // �� �ڽ� ��ü�� ������ ��Ʈ �̸��� �ҷ����� �Լ�

        //��Ʈ�� ��ϵ� �̹��� ��θ� ã�� �̹����� ��ȯ�ϴ� �Լ�
        protected Sprite AddOrGetImageDictionary(Dictionary<string, Sprite> imageDictionary, string imagePath, string imageName)
        {
            if (imageDictionary.ContainsKey(imageName))
            {
                return imageDictionary[imageName];
            }

            imagePath += imageName;
            var sprite = Resources.Load<Sprite>(imagePath);

            if (sprite == null)
            {
                Debug.LogWarning("�̹����� ã�� ���߽��ϴ�. in " + imageName);
                return null;
            }
            imageDictionary.Add(imageName, sprite);
            return imageDictionary[imageName];
        }

        // Base�� �θ��� ��ü���� ���������� ���Ǵ� �ڳ� ��Ʈ �ε� �Լ�
        // ��Ʈ �̸��� �´� �Լ��� �ε��ϰ� LoadChartDataTemplate�� ������ ��ü���� ��� �����ϰԲ� �Ľ����ش�.


        public void BackendChartDataLoad(AfterBackendLoadFunc afterBackendLoadFunc)
        {

            string chartFileName = GetChartFileName();
            string className = GetType().Name;
            bool isSuccess = false;
            string errorInfo = string.Empty;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            string chartId = string.Empty;

            if (!GameManager.Backend.Chart.ChartInfo.Dictionary.ContainsKey(chartFileName))
            {
                afterBackendLoadFunc(isSuccess, className, funcName, $"��Ʈ�� {chartFileName}�� ���� ������ �������� �ʽ��ϴ�.");
                return;
            }

            chartId = GameManager.Backend.Chart.ChartInfo.Dictionary[chartFileName];

            // [�ڳ�] ��Ʈ �ҷ����� �Լ�
            SendQueue.Enqueue(BackEnd.Backend.Chart.GetChartContents, chartId, callback => {
                try
                {
                    Debug.Log($"Backend.Chart.GetChartContents({chartId}) : {callback}");

                    if (callback.IsSuccess())
                    {
                        JsonData json = callback.FlattenRows();

                        LoadChartDataTemplate(json); // �� �ڽ� ��ü�� ������ ���ϰ� �Ľ� �Լ�

                        isSuccess = true;
                    }
                    else
                    {
                        errorInfo = callback.ToString();
                    }
                }
                catch (Exception e)
                {
                    errorInfo = e.ToString();
                }
                finally
                {
                    afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);
                }
            });
        }
        
    }

}