using System.Collections.Generic;

namespace BackendData.Post
{
    public class Item
    {
        public string InDate { get; private set; }
        public string Title { get; private set; }
        public string Content { get; private set; }
        public bool IsReceivable { get; private set; }

        public Dictionary<string, int> Rewards { get; private set; } = new();

        public Item(string inDate, string title, string content, bool isReceivable, Dictionary<string, int> rewards)
        {
            InDate = inDate;
            Title = title;
            Content = content;
            IsReceivable = isReceivable;
            Rewards = rewards;
        }

        public override string ToString()
        {
            var result = $"title : {Title}\ncontent : {Content}\ninDate : {InDate}\n";
            if (IsReceivable)
            {
                result += "���� ������\n";
                foreach (var kv in Rewards)
                {
                    result += $"| {kv.Key} : {kv.Value}��\n";
                }
            }
            else
            {
                result += "�������� �ʴ� ���� �������Դϴ�.";
            }

            return result;
        }
    }
}
