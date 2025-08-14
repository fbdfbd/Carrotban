using System.Collections.Generic;
using System.Collections.ObjectModel; 
using LitJson;

namespace BackendData.Chart.Chapter
{
    public class Manager : Base.Chart
    {
        private readonly Dictionary<int, Item> _dictionary = new();
        public IReadOnlyDictionary<int, Item> Dictionary => new ReadOnlyDictionary<int, Item>(_dictionary);

        public override string GetChartFileName()
        {
            return "CHAPTER";
        }

        protected override void LoadChartDataTemplate(JsonData json)
        {
            foreach (JsonData eachItemJson in json)
            {
                Item chapterItem = new Item(eachItemJson);

                if (!_dictionary.ContainsKey(chapterItem.ChapterID))
                {
                    _dictionary.Add(chapterItem.ChapterID, chapterItem);
                }
            }
        }

        public Item GetChapterItem(int chapterId)
        {
            _dictionary.TryGetValue(chapterId, out Item item);
            return item;
        }
    }
}