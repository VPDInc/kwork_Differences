using UnityEngine;

[CreateAssetMenu(menuName = "Objects/DailyReward", fileName = "New Daily Reward")]
public class DailyRewardData : ScriptableObject
{
    [SerializeField] private RewardEnum _type = RewardEnum.Soft;
    [SerializeField] [Min(1)] private int _count = 1;
    [SerializeField] private Sprite _image;

    public RewardEnum Type => _type;
    public int Count => _count;
    public Sprite Image => _image;
}
