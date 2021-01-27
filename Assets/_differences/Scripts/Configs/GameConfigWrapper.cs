using UnityEngine;

namespace _differences.Scripts.Configs
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Differences/Create GameConfig")]

    public class GameConfigWrapper: ScriptableObject
    {
        public GameConfig gameConfig;

        public string GetGameConfigAsJson(bool prettyPrint) => JsonUtility.ToJson(gameConfig, prettyPrint);

        public void SetGameConfigFromJson(string json) => gameConfig = JsonUtility.FromJson<GameConfig>(json);
    }
}
