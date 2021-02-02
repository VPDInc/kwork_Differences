using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace _differences.Scripts.PVPBot
{
    public class UIPvP : MonoBehaviour
    {
        [Inject] private BotFactory botFactory = default;
        [Inject] private Tournament _tournament = default;
        [Inject] private AvatarsPool _avatarPool = default;
        [Inject] private PlayerInfoController _infoController = default;

        [SerializeField] private List<UIPlayerHolderPvP> uIPlayers = new List<UIPlayerHolderPvP>();

        [SerializeField] private UIPlayerHolderPvP uiPlayer;
        [SerializeField] private Transform uiPlayerTransform;

        private int countDifferences = 5;

        public void BuildPvpMatch(Data data)
        {
            FindOpponents();
        }

        public void StartPvpMatch()
        {
            botFactory.StartGame();
        }

        private void FindOpponents()
        {
            var ui = Instantiate(uiPlayer, uiPlayerTransform);
            ui.Setup(delegate { });
            uIPlayers.Add(ui);
        }
    }
}