using UnityEngine;

[CreateAssetMenu(menuName = "Differences/New Stars Earnings Config", fileName = "StarsEarningConfig")]
public class StarsEarningConfig : ScriptableObject {
    public int StarsPerFoundDifference => _starsPerFoundDifference;
    public int StarsPerCompletedPicture => _starsPerCompletedPicture;

    [SerializeField] int _starsPerFoundDifference = 2;
    [SerializeField] int _starsPerCompletedPicture = 10;
}
