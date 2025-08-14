using System;
using BackEnd;
using UnityEngine;

namespace BackendData.Utils
{
    public static class BackendUtils
    {
        public static DateTime GetServerDateTime()
        {
            var bro = Backend.Utils.GetServerTime();
            if (bro.IsSuccess())
            {
                //yyyy-MM-ddTHH:mm:ssZ
                return DateTime.Parse(bro.GetReturnValuetoJSON()["utcTime"].ToString());
            }
            else
            {
                Debug.LogWarning("���� �ð� �޾ƿ��� ����! Ŭ���̾�Ʈ UTC �ð� ���.");
                return DateTime.UtcNow;
            }
        }

        public static string GetServerDateString()
        {
            DateTime now = GetServerDateTime();
            return now.ToString("yyyy-MM-dd");
        }
    }
}
