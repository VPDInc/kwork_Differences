using Airion.DailyRewards;

using UnityEngine;

[CreateAssetMenu(menuName = "Airion/Daily Rewards/New Currency Reward", fileName = "Reward")]
public class CurrencyReward : Reward {
    public int Amount => _amount;
    [SerializeField] int _amount = 1;
}
