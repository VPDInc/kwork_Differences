using Airion.DailyRewards;

using UnityEngine;

[CreateAssetMenu(menuName = "Airion/Daily Rewards/New Currency Reward", fileName = "Reward")]
public class CurrencyReward : Reward
{
    public enum TypeReward
    {
        Coin,
        Target
    }

    [SerializeField] private TypeReward _typeReawrd;
    [SerializeField] [Min(1)] private int _count = 1;

    public TypeReward Type => _typeReawrd;
    public int Count => _count;

}
