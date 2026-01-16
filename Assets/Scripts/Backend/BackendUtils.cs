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
                Debug.LogWarning("서버 시간 받아오기 실패! 클라이언트 UTC 시간 사용.");
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
