// Copyright 2013-2022 AFI, INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using BackEnd;
using LitJson;
using Unity.VisualScripting;
using UnityEngine;

namespace BackendData.Base
{
    //===============================================================
    // ���� �������� �ҷ������ ���⿡ ���� �������� ������ ���� Ŭ����
    //===============================================================
    public abstract class GameData : Normal
    {
        private string _inDate;

        public string GetInDate()
        {
            return _inDate;
        }

        public bool IsChangedData { get; set; }

        public abstract string GetTableName(); // �ڽ� ��ü�� ������ ���̺� �̸� �������� �Լ�
        public abstract string GetColumnName(); // �ڽ� ��ü�� ������ �÷� �̸� �������� �Լ�
        public abstract Param GetParam(); // �ڽ� ��ü�� ������ ������Ʈ�� ���� Param�� �������� �Լ�

        protected abstract void InitializeData(); // �ڽ� ��ü�� ������ ������ �ʱ�ȭ�� ���ִ� �Լ�

        protected abstract void SetServerDataToLocal(JsonData gameDataJson); // �ڽ� ��ü�� ������ �ҷ����� �Լ� ȣ�� ���� �� ���̺� �°� �Ľ��ϴ� �Լ�

        // 
        public void BackendGameDataLoad(AfterBackendLoadFunc afterBackendLoadFunc)
        {
            string tableName = GetTableName();
            string columnName = GetColumnName();
            string className = tableName;

            bool isSuccess = false;
            string errorInfo = string.Empty;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            // [�ڳ�] �� �������� �ҷ����� �Լ�
            SendQueue.Enqueue(Backend.GameData.GetMyData, tableName, new Where(), callback => {
                try
                {
                    Debug.Log($"Backend.GameData.GetMyData({tableName}) : {callback}");

                    if (callback.IsSuccess())
                    {
                        // �ҷ��� �����Ͱ� �ϳ��� ������ ���
                        if (callback.FlattenRows().Count > 0)
                        {

                            // ���� ������Ʈ�� ���� �� �������� indate�� ����
                            _inDate = callback.FlattenRows()[0]["inDate"].ToString();

                            //Dictionary �� ������ ������ ���� �÷�����  �������� ���
                            if (string.IsNullOrEmpty(columnName))
                            {
                                // FlattenRows�� ���, ���ϰ��� ["S"], ["N"]���� ������ Ÿ�԰��� ������ ��, Json�� ����
                                SetServerDataToLocal(callback.FlattenRows()[0]);
                            }

                            else
                            {
                                // �������� �ʾ��� ���(UserData)
                                // columnName���� ������ ��, Json�� ����
                                SetServerDataToLocal(callback.FlattenRows()[0][columnName]);
                            }

                            isSuccess = true;
                            // �ҷ����Ⱑ ���� ���Ŀ� ȣ��Ǵ� �Լ� ȣ��
                            afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);
                        }
                        else
                        {
                            // �ҷ��� �����Ͱ� ���� ���, ������ �������� �ʴ� ���
                            // �����͸� ���� ����
                            BackendInsert(afterBackendLoadFunc);
                        }
                    }
                    else
                    {
                        // ������ ���� ���� ������� ������ �߻����� ���(������ �����Ͱ� ������ ��������)
                        errorInfo = callback.ToString();
                        afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);
                    }
                }
                catch (Exception e)
                {
                    // ���ܰ� �߻����� ���
                    // �Ľ� ���е�
                    errorInfo = e.ToString();
                    afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);
                }
            });
        }


        // ������ �����Ͱ� �������� ���� ���, �����͸� ���� ����
        private void BackendInsert(AfterBackendLoadFunc afterBackendLoadFunc)
        {
            string tableName = GetTableName();
            bool isSuccess = false;
            string errorInfo = string.Empty;
            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            // ������ �ʱ�ȭ(�� �ڽ� ��ü�� ����)
            InitializeData();

            // [�ڳ�] ���� ���� ���� �Լ�
            SendQueue.Enqueue(Backend.GameData.Insert, tableName, GetParam(), callback => {
                try
                {
                    Debug.Log($"Backend.GameData.Insert({tableName}) : {callback}");

                    if (callback.IsSuccess())
                    {
                        isSuccess = true;
                        _inDate = callback.GetInDate();
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

        // ������Ʈ�� �Ϸ�� ���� ���ϰ��� �Բ� ȣ��Ǵ� �Լ�
        public delegate void AfterCallback(BackendReturnObject callback);

        // �ش� ���̺� ������ ������Ʈ
        public void Update(AfterCallback afterCallback)
        {
            SendQueue.Enqueue(Backend.GameData.UpdateV2, GetTableName(), GetInDate(), Backend.UserInDate, GetParam(),
                callback => {
                    Debug.Log($"Backend.GameData.UpdateV2({GetTableName()}, {GetInDate()}, {Backend.UserInDate}) : {callback}");
                    afterCallback(callback);
                });
        }

        // �ش� ���̺� ������Ʈ�� ������ Ʈ�����(�ѹ��� ���� ���̺� ����)���� ����� ����
        public TransactionValue GetTransactionUpdateValue()
        {
            return TransactionValue.SetUpdateV2(GetTableName(), GetInDate(), Backend.UserInDate, GetParam());
        }

        // �ش� ���̺� ������Ʈ�� ������ Ʈ�����(�ѹ��� ���� ���̺� ����)���� ����� ����
        public TransactionValue GetTransactionGetValue()
        {
            Where where = new Where();
            where.Equal("owner_inDate", Backend.UserInDate);
            return TransactionValue.SetGet(GetTableName(), where);
        }

        public void BackendGameDataLoadByTransaction(JsonData gameDataJson, AfterBackendLoadFunc afterBackendLoadFunc)
        {
            string columnName = GetColumnName();
            string errorInfo = string.Empty;
            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            try
            {
                // ���� ������Ʈ�� ���� �� �������� inDate�� ����
                _inDate = gameDataJson["inDate"].ToString();

                //Dictionary �� ������ ������ ���� �÷�����  �������� ���
                if (string.IsNullOrEmpty(columnName))
                {
                    // FlattenRows�� ���, ���ϰ��� ["S"], ["N"]���� ������ Ÿ�԰��� ������ ��, Json�� ����
                    SetServerDataToLocal(gameDataJson);
                }
                else
                {
                    // �������� �ʾ��� ���(UserData)
                    // columnName���� ������ ��, Json�� ����
                    SetServerDataToLocal(gameDataJson[columnName]);
                }
                afterBackendLoadFunc(true, className, funcName, errorInfo);
            }
            catch (Exception e)
            {
                errorInfo = e.ToString();
                afterBackendLoadFunc(false, className, funcName, errorInfo);

            }
        }
    }
}