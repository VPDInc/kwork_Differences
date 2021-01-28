using _differences.Scripts.PVPBot;
using System;
using UnityEngine;

namespace _differences.Scripts.Configs
{
    [Serializable]
    public struct GameConfig
    {
        public BotConfig BotConfig;
    }

    [Serializable]
    public struct BotConfig
    {
        [Header("InitialDifference values")]
        public int MinTimeFindInitialDifference;
        public int MaxTimeFindInitialDifference;
        [Header("NextDifference values")]
        public int MinTimeFindNextDifference;
        public int MaxTimeFindNextDifference;

        //public int 
        //[Header("")]
        public DifficultyBot[] BotDifficulties;
    }

    [Serializable]
    public struct DifficultyBot
    {
        public string DifficultyValue;
        public float PercentValue;
    }


}
