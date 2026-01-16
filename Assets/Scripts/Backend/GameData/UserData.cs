using System;
using BackEnd;
using LitJson;
using UnityEngine;

namespace BackendData.GameData
{
    /// <summary>
    /// 유저의 스테이지, 재화, 언어 등 기본 데이터를 보관하는 게임 데이터 모델.
    /// </summary>
    public partial class UserData : Base.GameData
    {
        public int Level { get; private set; }
        public int Gold { get; private set; }
        public float Exp { get; private set; }
        public int MaxExp { get; private set; }
        public int CurrentStageKey { get; private set; }
        public int PresentStageKey { get; private set; }
        public GameLanguage Language { get; private set; }


        protected override void InitializeData()
        {
            Level = 1;
            Gold = 0;
            MaxExp = 100;         
            CurrentStageKey = 101;
            PresentStageKey = 100;
            Language = GameLanguage.KOR;
        }


        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            Level = int.Parse(gameDataJson["Level"].ToString());
            Exp = float.Parse(gameDataJson["Exp"].ToString());
            MaxExp = int.Parse(gameDataJson["MaxExp"].ToString());
            Gold = int.Parse(gameDataJson["Gold"].ToString());
            CurrentStageKey = int.Parse(gameDataJson["CurrentStageKey"].ToString());
            PresentStageKey = int.Parse(gameDataJson["PresentStageKey"].ToString());
            Language = (GameLanguage)Enum.Parse(typeof(GameLanguage),
                                                       gameDataJson["Language"].ToString(), true);
        }


        public override string GetTableName() => "USER_DATA";
        public override string GetColumnName() => null;

        public override Param GetParam()
        {
            Param param = new Param();
            param.Add("Level", Level);
            param.Add("Gold", Gold);
            param.Add("Exp", Exp);
            param.Add("MaxExp", MaxExp);
            param.Add("CurrentStageKey", CurrentStageKey);
            param.Add("PresentStageKey", PresentStageKey);
            param.Add("Language", Language.ToString());
            return param;
        }


        public void UpdateUserExp(float exp)
        {
            IsChangedData = true;
            Exp += exp;

            if (Exp >= MaxExp)                      
            {
                while (Exp >= MaxExp)               
                {
                    LevelUp();
                }
            }
        }

        public void UpdateUserGold(int gold)
        {
            IsChangedData = true;
            Gold += gold;
        }

        public void UpdateStageClear(int clearedKey)
        {
            int chapter = clearedKey / 100;
            int stage = clearedKey % 100;

            if (stage == 10)
            {
                chapter++;
                stage = 1;
            }
            else
            {
                stage++;
            }

            int newNextStageKey = chapter * 100 + stage;

            if (newNextStageKey > CurrentStageKey)
            {
                CurrentStageKey = newNextStageKey;
                IsChangedData = true;
            }
        }

        public void UpdatePresentStage(int key)
        {
            PresentStageKey = key;
            IsChangedData = true;
        }


        private void LevelUp()
        {
            Exp -= MaxExp;
            MaxExp = Mathf.RoundToInt(MaxExp * 1.1f);
            Level++;
        }
    }
}
