using System;
using UnityEngine;
using Zenject;

namespace _differences.Scripts.PVPBot
{
    public class UiPlayerHolderPvP : MonoBehaviour
    {
        [Inject] private readonly BotFactory botFactory = default;

        public void Setup(Action alreadyBuild)
        {
            var currentActiveBot = botFactory.GetBot();
            currentActiveBot.CountDifferences = 5; //????? countDifferences;
            currentActiveBot.BotDifficulty = BotDifficulty.Easy;
            currentActiveBot.SuccessFindDifference += CurrentActiveBotSuccessFindDifference;
            currentActiveBot.AllFindDifferences += CurrentActiveBotAllFindDifferences;

            alreadyBuild?.Invoke();
        }

        private void CurrentActiveBotAllFindDifferences(Bot obj)
        {

        }

        private void CurrentActiveBotSuccessFindDifference(DifferencesData obj)
        {

        }
    }
}
