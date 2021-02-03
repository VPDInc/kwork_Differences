using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Объекты/Карта вознограждения", fileName = "New Reward Card")]
public class RewardCardData : ScriptableObject
{
    [SerializeField] [Min(1)] private int _numberOfDay;
    [SerializeField] private DailyRewardData[] _rewards;

    public int NumberOfDay => _numberOfDay;
    public DailyRewardData[] Rewards => _rewards;
}
