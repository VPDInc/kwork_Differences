using UnityEngine;

[CreateAssetMenu(fileName = "TournamentRewards", menuName = "Differences/New Tournament Rewards")]
public class TournamentRewards : ScriptableObject {
    [SerializeField] int[] _rewards = {0};
    [SerializeField] int _lastPlaces = 10;
    
    public int GetRewardByPlace(int place) {
        if (place > _lastPlaces)
            return 0;
        
        var placeClamped = Mathf.Clamp(place, 0, _rewards.Length-1);
        return _rewards[placeClamped];
    }
}
