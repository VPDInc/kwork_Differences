using UnityEngine;

namespace _differences.Scripts.Configs
{
    //TODO 01.02.2021 NEED REMOVE
    public class GameConfigController : MonoBehaviour
    {
        [SerializeField] private GameConfigWrapper gameConfig;

        /// <summary>
        /// Use only for test mechanics in Editor 
        /// </summary>
        public GameConfig GetLoccalyConfig => gameConfig.gameConfig;
    }
}
