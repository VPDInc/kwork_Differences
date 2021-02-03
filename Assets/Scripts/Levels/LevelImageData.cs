using UnityEngine;

namespace Differences.Levels
{
    public enum DifficultyLevel
    {
        SuperEasy,
        Normal
    }

    [CreateAssetMenu(menuName = "Objects/LevelImage", fileName = "New Level Image")]
    public class LevelImageData : ScriptableObject
    {
        [SerializeField] private int _numberImage;

        [SerializeField] private DifficultyLevel _complexity = DifficultyLevel.Normal;
        [SerializeField] [Range(5, 10)] private int _countDifferences = 5;

        public int NumberImage => _numberImage;

        public DifficultyLevel Complexity => _complexity;
        public int CountDifferences => _countDifferences;
    }
}
