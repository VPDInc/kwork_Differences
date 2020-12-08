using Airion.DailyRewards;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class UIDay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _dayNum = default;
    [SerializeField] TextMeshProUGUI _rewardAmount = default;
    [SerializeField] Image _chest = default;
    [SerializeField] Sprite _openedEmpty = default;
    [SerializeField] Sprite _openedFull = default;
    [SerializeField] Sprite _closed = default;

    public void SetInfo(DayInfo info)
    {
        if (info.Status == DayStatus.NotOpened && info.Additional.HasFlag(AdditionalDaySetting.Today)) {
            _chest.sprite = _openedFull;
        } else  if (info.Status == DayStatus.NotOpened) {
            _chest.sprite = _closed;
        } else {
            _chest.sprite = _openedEmpty;
        }
        
        _dayNum.text = "Day " + (info.DayNum + 1).ToString();
        if (info.Additional.HasFlag(AdditionalDaySetting.Today)) {
            _dayNum.text = "Today";
        }
        
        if (info.Additional.HasFlag(AdditionalDaySetting.Tomorrow)) {
            _dayNum.text = "Tomorrow";
        }

        _rewardAmount.text = ((CurrencyReward)info.Reward).Count.ToString();
    }
}
