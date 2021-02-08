using System;

namespace Airion.DailyRewards
{
    public struct DayInfo
    {
        public int DayNum;
        public DayStatus Status;
        public AdditionalDaySetting Additional;
        public Reward Reward;
        
        public override string ToString() =>
            $"Day: {DayNum} Status: {Status} Additional: {Additional}";
    }

    public enum DayStatus
    {
        Opened,
        NotOpened
    }

    [Flags]
    public enum AdditionalDaySetting
    {
        None = 0,
        Today = 1,
        Tomorrow = 2
    }
}