using System;
using System.Collections.Generic;
using System.Reflection;
using BackEnd;
using LitJson;
using UnityEngine;

namespace BackendData.Post
{
    public class Manager : Base.Normal
    {
        private readonly List<Item> _postList = new();
        public IReadOnlyList<Item> PostList => _postList;

        public delegate void AfterGetPostListFunc(bool isSuccess, string errorInfo);
        public delegate void AfterReceivePostFunc(bool isSuccess, string errorInfo);

        public override void BackendLoad(AfterBackendLoadFunc afterBackendLoadFunc)
        {
            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;


            GetPostList(PostType.Admin, (isSuccess, errorInfo) =>
            {
                afterBackendLoadFunc?.Invoke(isSuccess, className, funcName, errorInfo);
            });
        }


        public void GetPostList(PostType postType, AfterGetPostListFunc onComplete)
        {
            bool isSuccess = false;
            string errorInfo = string.Empty;

            SendQueue.Enqueue(Backend.UPost.GetPostList, postType, callback =>
            {
                try
                {
                    Debug.Log($"[Post.Manager] GetPostList({postType}) => {callback}");

                    if (!callback.IsSuccess())
                        throw new Exception(callback.ToString());

                    _postList.Clear();

                    JsonData postListJson = callback.GetFlattenJSON()["postList"];
                    if (postListJson == null || postListJson.Count == 0)
                    {
                        isSuccess = true;
                        return;
                    }

                    foreach (JsonData postJson in postListJson)
                    {
                        string title = postJson["title"].ToString();
                        string content = postJson["content"].ToString();
                        string inDate = postJson["inDate"].ToString();

                        bool receivable = false;
                        Dictionary<string, int> rewards = new();

                        if (postType == PostType.User)
                        {
                            if (postJson["itemLocation"]["tableName"].ToString() == "USER_DATA" &&
                                postJson["itemLocation"]["column"].ToString() == "inventory")
                            {
                                foreach (string key in postJson["item"].Keys)
                                {
                                    int cnt = int.Parse(postJson["item"][key].ToString());
                                    if (rewards.ContainsKey(key)) rewards[key] += cnt; else rewards.Add(key, cnt);
                                    receivable = true;
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"[Post.Manager] Unsupported itemLocation : {postJson["itemLocation"].ToJson()}");
                            }
                        }
                        else 
                        {
                            foreach (JsonData itemJson in postJson["items"])
                            {
                                string itemName = itemJson["item"]["itemName"].ToString();
                                int cnt = int.Parse(itemJson["itemCount"].ToString());
                                if (rewards.ContainsKey(itemName)) rewards[itemName] += cnt; else rewards.Add(itemName, cnt);
                                receivable = true;
                            }
                        }

                        _postList.Add(new Item(inDate, title, content, receivable, rewards));
                    }

                    for (int i = 0; i < _postList.Count; ++i)
                    {
                        Debug.Log($"[Post.Manager] Loaded[{i}]\n{_postList[i]}");
                    }

                    isSuccess = true;
                }
                catch (Exception e)
                {
                    errorInfo = e.ToString();
                }
                finally
                {
                    onComplete?.Invoke(isSuccess, errorInfo);
                }
            });
        }

        public void ReceivePostAtIndex(PostType postType, int index, AfterReceivePostFunc onComplete)
        {
            if (index < 0 || index >= _postList.Count)
            {
                onComplete?.Invoke(false, $"Invalid post index : {index}");
                return;
            }

            bool isSuccess = false;
            string errorInfo = string.Empty;
            string targetInDate = _postList[index].InDate;

            SendQueue.Enqueue(Backend.UPost.ReceivePostItem, postType, targetInDate, callback =>
            {
                try
                {
                    Debug.Log($"[Post.Manager] ReceivePostItem({postType},{targetInDate}) => {callback}");

                    if (!callback.IsSuccess())
                        throw new Exception(callback.ToString());

                    _postList.RemoveAt(index);

                    JsonData postItemsJson = callback.GetFlattenJSON()["postItems"];
                    if (postItemsJson != null && postItemsJson.Count > 0)
                    {
                        SetLocalRewards(postItemsJson);
                    }

                    isSuccess = true;
                }
                catch (Exception e)
                {
                    errorInfo = e.ToString();
                }
                finally
                {
                    onComplete?.Invoke(isSuccess, errorInfo);
                }
            });
        }

        public void ReceiveAllPosts(PostType postType, AfterReceivePostFunc onComplete)
        {
            bool isSuccess = false;
            string errorInfo = string.Empty;

            SendQueue.Enqueue(Backend.UPost.ReceivePostItemAll, postType, callback =>
            {
                try
                {
                    Debug.Log($"[Post.Manager] ReceivePostItemAll({postType}) => {callback}");

                    if (!callback.IsSuccess())
                        throw new Exception(callback.ToString());

                    JsonData postItemsJson = callback.GetFlattenJSON()["postItems"];
                    if (postItemsJson != null && postItemsJson.Count > 0)
                    {
                        SetLocalRewards(postItemsJson);
                    }

                    _postList.Clear();
                    isSuccess = true;
                }
                catch (Exception e)
                {
                    errorInfo = e.ToString();
                }
                finally
                {
                    onComplete?.Invoke(isSuccess, errorInfo);
                }
            });
        }

        private void SetLocalRewards(JsonData items)
        {
            
        }
    }
}
