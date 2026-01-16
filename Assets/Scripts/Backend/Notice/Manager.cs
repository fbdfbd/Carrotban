using System;
using System.Collections.Generic;
using System.Reflection;
using BackEnd;
using LitJson;
using UnityEngine;

namespace BackendData.Notice
{
    public class Manager : Base.Normal
    {
        private Dictionary<string, Item> _dictionary = new Dictionary<string, Item>();
        public IReadOnlyDictionary<string, Item> Dictionary => _dictionary;

        public delegate void AfterGetNoticeFunc(bool isSuccess, string errorInfo);

        public override void BackendLoad(AfterBackendLoadFunc afterBackendLoadFunc)
        {
            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            GetNoticeList((isSuccess, errorInfo) =>
            {
                afterBackendLoadFunc.Invoke(isSuccess, className, funcName, errorInfo);
            });
        }

        public void GetNoticeList(AfterGetNoticeFunc afterNoticeLoadingFunc)
        {
            bool isSuccess = false;
            string errorInfo = string.Empty;

            SendQueue.Enqueue(Backend.Notice.NoticeList, 10, callback =>
            {
                try
                {
                    Debug.Log($"Backend.Notice.NoticeList: {callback}");

                    if (!callback.IsSuccess())
                        throw new Exception(callback.ToString());

                    JsonData noticeListJson = callback.FlattenRows();

                    for (int i = 0; i < noticeListJson.Count; i++)
                    {
                        string inDate = noticeListJson[i]["inDate"].ToString();
                        if (_dictionary.ContainsKey(inDate)) continue;

                        string title = noticeListJson[i]["title"].ToString();
                        string content = noticeListJson[i]["content"].ToString();
                        string postingDate = noticeListJson[i]["postingDate"].ToString();

                        Item noticeItem = new Item(
                            title: title,
                            date: postingDate,
                            content: content
                        );

                        _dictionary.Add(title, noticeItem);
                    }

                    isSuccess = true;
                }
                catch (Exception e)
                {
                    errorInfo = e.ToString();
                }
                finally
                {
                    afterNoticeLoadingFunc.Invoke(isSuccess, errorInfo);
                }
            });
        }
    }
}
