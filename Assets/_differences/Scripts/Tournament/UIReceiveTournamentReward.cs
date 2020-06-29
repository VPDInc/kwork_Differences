using System;

using Airion.DailyRewards;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;

public class UIReceiveTournamentReward : MonoBehaviour {
    public event Action<int> Received;
    
    [SerializeField] TextMeshProUGUI _rewardedAmountText = default;
    
    [SerializeField] UIView _view = default;

    int _amount;

    public void Show(int amount) {
        _amount = amount;
        _rewardedAmountText.text = amount.ToString();
        _view.Show();
    }

    public void Hide() {
        _view.Hide();
    }

    public void OnReceiveClick() {
        Received?.Invoke(_amount);
    }
}
