using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Объекты/Ежедневная награда", fileName = "New Daily Reward")]
public class DailyRewardData : ScriptableObject
{
    public enum TypeReward
    {
        Coin,
        Target
    }

    [SerializeField] private TypeReward _type = TypeReward.Coin;
    [SerializeField] [Min(1)] private int _count = 1;
    [SerializeField] private Sprite _image;

    public TypeReward Type => _type;
    public int Count => _count;
    public Sprite Image => _image;
}
