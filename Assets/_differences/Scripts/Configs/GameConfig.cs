using _differences.Scripts.PVPBot;
using System;
using UnityEngine;

namespace _differences.Scripts.Configs
{
    [Serializable]
    public struct GameConfig
    {
        public DifficultyBot[] Difficulties;
    }

    [Serializable]
    public struct DifficultyBot
    {
        public BotDifficulty DifficultyValue;
        public float PercentValue;
    }
}
