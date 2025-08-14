using System;
using BackEnd;
using LitJson;

namespace BackendData.GameData
{
    public partial class UserHeart : Base.GameData
    {
        public int Heart { get; private set; }
        public int RechargeMinutes { get; private set; }
        public DateTime LastChargedTime { get; private set; }

        public const int MaxHeart = 5;

        public void RefreshHeart()
        {
            var bro = Backend.Utils.GetServerTime();
            if (!bro.IsSuccess()) return;

            DateTime serverNow = DateTime.Parse(
                bro.GetReturnValuetoJSON()["utcTime"].ToString(),
                null,
                System.Globalization.DateTimeStyles.AdjustToUniversal
            );

            if (Heart >= MaxHeart) return;
            if (RechargeMinutes <= 0) RechargeMinutes = 1;

            int cycleSec = RechargeMinutes * 60;
            TimeSpan elapsed = serverNow - LastChargedTime;
            if (elapsed.TotalSeconds < cycleSec) return;

            int addCount = (int)(elapsed.TotalSeconds / cycleSec);
            if (addCount <= 0) return;

            int targetHeart = Math.Min(MaxHeart, Heart + addCount);
            int actualAdd = targetHeart - Heart;
            if (actualAdd <= 0) return;

            Heart = targetHeart;

            int remainder = (int)(elapsed.TotalSeconds % cycleSec);
            LastChargedTime = serverNow.AddSeconds(-remainder);

            IsChangedData = true;
        }

        public bool TryUseHeart()
        {
            RefreshHeart();
            if (Heart <= 0) return false;

            int prevHeart = Heart;
            Heart = Math.Max(0, Heart - 1);

            if (prevHeart == MaxHeart && Heart == MaxHeart - 1)
            {
                var bro = Backend.Utils.GetServerTime();
                LastChargedTime = bro.IsSuccess()
                    ? DateTime.Parse(
                        bro.GetReturnValuetoJSON()["utcTime"].ToString(),
                        null,
                        System.Globalization.DateTimeStyles.AdjustToUniversal
                      )
                    : DateTime.UtcNow;
            }

            IsChangedData = true;
            return true;
        }

        public void AddHeart(int amount)
        {
            if (amount == 0) return;
            Heart = Math.Clamp(Heart + amount, 0, MaxHeart);
            IsChangedData = true;
        }

        public void AddHeartPaid(int amount)
        {
            if (amount == 0) return;
            Heart = Math.Max(0, Heart + amount);
            IsChangedData = true;
        }

        public TimeSpan GetNextHeartRemainTime()
        {
            if (Heart >= MaxHeart) return TimeSpan.Zero;

            var bro = Backend.Utils.GetServerTime();
            if (!bro.IsSuccess()) return TimeSpan.Zero;

            DateTime serverNow = DateTime.Parse(
                bro.GetReturnValuetoJSON()["utcTime"].ToString(),
                null,
                System.Globalization.DateTimeStyles.AdjustToUniversal
            );

            if (RechargeMinutes <= 0) RechargeMinutes = 1;

            int cycleSeconds = RechargeMinutes * 60;
            double elapsedSec = (serverNow - LastChargedTime).TotalSeconds;
            if (elapsedSec < 0) elapsedSec = 0;

            int remainSeconds = cycleSeconds - (int)(elapsedSec % cycleSeconds);
            if (remainSeconds == cycleSeconds) remainSeconds = 0;

            return TimeSpan.FromSeconds(remainSeconds);
        }

        protected override void InitializeData()
        {
            RechargeMinutes = (RechargeMinutes <= 0) ? 1 : RechargeMinutes;
            Heart = MaxHeart;

            var bro = Backend.Utils.GetServerTime();
            LastChargedTime = bro.IsSuccess()
                ? DateTime.Parse(
                    bro.GetReturnValuetoJSON()["utcTime"].ToString(),
                    null,
                    System.Globalization.DateTimeStyles.AdjustToUniversal
                  )
                : DateTime.UtcNow;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            if (gameDataJson.ContainsKey("Heart") && int.TryParse(gameDataJson["Heart"].ToString(), out var h))
                Heart = Math.Max(0, h);
            else
                Heart = MaxHeart;

            if (gameDataJson.ContainsKey("RechargeMinutes") &&
                int.TryParse(gameDataJson["RechargeMinutes"].ToString(), out var rm) && rm > 0)
                RechargeMinutes = rm;
            else
                RechargeMinutes = 1;

            if (gameDataJson.ContainsKey("LastChargedTime"))
                LastChargedTime = DateTime.Parse(gameDataJson["LastChargedTime"].ToString(), null,
                    System.Globalization.DateTimeStyles.AdjustToUniversal);
            else
                LastChargedTime = DateTime.UtcNow;
        }

        public override string GetTableName() => "USER_HEART";
        public override string GetColumnName() => null;

        public override Param GetParam()
        {
            Param param = new Param();
            param.Add("Heart", Heart);
            param.Add("RechargeMinutes", RechargeMinutes);
            param.Add("LastChargedTime", LastChargedTime.ToString("o"));
            return param;
        }
    }
}
