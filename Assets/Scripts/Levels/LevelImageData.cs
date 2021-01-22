using UnityEngine;

public enum ComplexityLevel
{
    SuperEasy,
    Normal
}

[CreateAssetMenu(menuName = "Objects/LevelImage", fileName = "New Level Image")]
public class LevelImageData : ScriptableObject
{
    [SerializeField] private int _numberImage;

    [SerializeField] private ComplexityLevel _complexity = ComplexityLevel.Normal;
    [SerializeField] [Range(5, 10)] private int _countDifferences = 5;

    public int NumberImage => _numberImage;

    public ComplexityLevel Complexity => _complexity;
    public int CountDifferences => _countDifferences;
}
