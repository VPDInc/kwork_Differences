using System;

using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "TournamentRewards", menuName = "Differences/New Tournament Rewards")]
public class TournamentRewards : ScriptableObject {
    [SerializeField] RewardContainer[] _rewards = default;
    [FormerlySerializedAs("_lastPlaces")] [SerializeField]
    int _lastRewardForNextPlaces = 10;

    [Serializable]
    class RewardContainer {
        public RewardInfo[] RewardInfos;
    }

    public RewardInfo[] GetRewardByPlace(int place) {
        if (place > _lastRewardForNextPlaces)
            return null;

        var placeClamped = GetClampedPlace(place);
        return _rewards[placeClamped].RewardInfos;
    }

    int GetClampedPlace(int place) {
        return Mathf.Clamp(place, 0, _rewards.Length - 1);
    }
}

[Serializable]
public class RewardInfo {
    public RewardEnum RewardType = default;
    public int Amount = default;
}