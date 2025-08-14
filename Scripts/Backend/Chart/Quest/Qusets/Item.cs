using LitJson;
using System;
using UnityEngine;

namespace BackendData.Chart.Quests
{
    public class Item
    {
        public int QuestID { get; private set; }
        public string Name_KR { get; private set; }
        public string Name_ENG { get; private set; }
        public string Des_KR { get; private set; }
        public string Des_ENG { get; private set; }
        public QuestType QuestType { get; private set; }
        public int PreqQuestID { get; private set; } 
        public int PreqClearStageID { get; private set; } 

        public Item(JsonData json)
        {
            QuestID = int.Parse(json["QuestID"].ToString());
            Name_KR = json["Name_KR"].ToString();
            Name_ENG = json["Name_ENG"].ToString();
            Des_KR = json["Des_KR"].ToString();
            Des_ENG = json["Des_ENG"].ToString();

            if (!Enum.TryParse<QuestType>(json["QuestType"].ToString(), out var questType))
            {
                throw new Exception($"Q{QuestID} - 지정되지 않은 QuestType 입니다.");
            }

            this.QuestType = questType;

            PreqQuestID = int.Parse(json["PreqQuestID"].ToString());
            PreqClearStageID = int.Parse(json["PreqClearStageID"].ToString());
        }
    }
}