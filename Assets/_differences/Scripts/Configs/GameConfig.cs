using _differences.Scripts.PVPBot;
using System;
using UnityEngine;

namespace _differences.Scripts.Configs
{
    [Serializable]
    public struct GameConfig
    {
        public BotConfig BotConfig;
        public int Version;
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

        [Header("TimeFindLastDifference values")]
        public int MinTimeFindLastDifference; //70
        public int MaxTimeFindLastDifference; //130

        //закончилось внремья на раунд при последней разницы от 5 до 10 2 переменных


        //Сравнение сколько разниц найдено у бота и игрока, 5 елементов
        public CompareDifferences[] CompareDifferences;
        

        //ускорений режим от 2 до 10 секунд

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


    [Serializable]
    public struct CompareDifferences
    {
        public int CountDiffernces;
        public int PercentStartFastFindDifferences;
    }

}
