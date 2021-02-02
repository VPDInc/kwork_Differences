using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace _differences.Scripts.PVPBot
{
    public class UIPvP : MonoBehaviour
    {
        [Inject] private readonly BotFactory botFactory = default;
        [Inject] private PlayerInfoController infoController = default;
        [Inject] private AvatarsPool avatarPool = default;
        [Inject] private Tournament tournament = default;

        private int countDifferences = 5;

        [SerializeField] private UiPlayerHolderPvP uiPlayer;

        [SerializeField] private List<UiPlayerHolderPvP> uIPlayers = new List<UiPlayerHolderPvP>();
        [SerializeField] private Transform uiPlayerTransform;

        public void BuildPvpMatch(Data data)
        {
            FindOpponents();
        }

        public void StartPvpMatch()
        {
            botFactory.InitializationBots();
        }

        private void FindOpponents()
        {
            var ui = Instantiate(uiPlayer, uiPlayerTransform);
            ui.Setup(delegate { });
            uIPlayers.Add(ui);
        }
    }
}