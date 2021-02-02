using System;
using UnityEngine;
using Zenject;

namespace _differences.Scripts.PVPBot
{
    public class UIPlayerHolderPvP : MonoBehaviour
    {
        [Inject] private BotFactory botFactory = default;

        public void Setup(Action allreadyBuild)
        {
            var currentActiveBot = botFactory.GetBot();
            currentActiveBot.CountDifferences = 5; //????? countDifferences;
            currentActiveBot.BotDifficulty = BotDifficulty.Easy;
            currentActiveBot.SuccessFindDifference += CurrentActiveBotSuccessFindDifference;
            currentActiveBot.AllFindDiffrences += CurrentActiveBot_AllFindDiffrences;

            allreadyBuild?.Invoke();
        }

        private void CurrentActiveBot_AllFindDiffrences(Bot obj)
        {

        }

        private void CurrentActiveBotSuccessFindDifference(DifferencesData obj)
        {

        }
    }
}
