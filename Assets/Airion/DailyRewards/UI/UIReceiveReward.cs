using System;

using Airion.DailyRewards;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;

public class UIReceiveReward : MonoBehaviour {
    public event Action Received;
    
    [SerializeField] TextMeshProUGUI _rewardedAmountText = default;
    
    [SerializeField] UIView _view = default;

    public void Show(Reward reward) {
        _rewardedAmountText.text = ((CurrencyReward)reward).Amount.ToString();
        _view.Show();
    }

    public void Hide() {
        _view.Hide();
    }

    public void OnReceiveClick() {
        Received?.Invoke();
    }

}
