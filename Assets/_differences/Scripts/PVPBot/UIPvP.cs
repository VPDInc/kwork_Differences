using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _differences.Scripts.PVPBot
{
    public class UIPvP : MonoBehaviour
    {
        [Inject] private BotController botController = default;
        [Inject] private Tournament _tournament = default;
        [Inject] private AvatarsPool _avatarPool = default;
        [Inject] private PlayerInfoController _infoController = default;

        private Bot currentActiveBot = default;

        private int countDifferences = 5;

        public void BuildPvpMatch(Action allreadyBuild)
        {
            FindOpponents();
            BuildBot(allreadyBuild);
        }

        public void StartPvpMatch()
        {
            currentActiveBot.Start();
        }

        private void FindOpponents()
        {

        }

        private void BuildBot(Action allreadyBuild)
        {
            currentActiveBot = botController.GetBot();
            currentActiveBot.CountDifferences = countDifferences;
            currentActiveBot.SuccessFindDifference += CurrentActiveBotSuccessFindDifference;
            currentActiveBot.AllFindDiffrences += CurrentActiveBotAllFindDiffrences;

            allreadyBuild?.Invoke();
        }

        private void CurrentActiveBotAllFindDiffrences()
        {
            //end game point for bot
        }

        private void CurrentActiveBotSuccessFindDifference(DifferencesData obj)
        {

        }
    }
}