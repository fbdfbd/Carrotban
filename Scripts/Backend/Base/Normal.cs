// Copyright 2013-2022 AFI, INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;
using BackEnd;
using UnityEngine;

namespace BackendData.Base
{
    //===================================================================
    // ��Ʈ, ���������� �ϰ������� ������ ������ �����ϱ� ���� ����� ���̽� Ŭ����
    //==================================================================
    public class Normal
    {

        // LoadingScene���� ����ϴ� �ҷ����Ⱑ �Ϸ�� ���Ŀ� ȣ��Ǵ� �Լ�
        public delegate void AfterBackendLoadFunc(bool isSuccess, string className, string functionName, string errorInfo);

        // �⺻���� ����
        public virtual void BackendLoad(AfterBackendLoadFunc afterBackendLoadFunc)
        {
            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;
            afterBackendLoadFunc(true, className, funcName, String.Empty);
        }
    }
}
