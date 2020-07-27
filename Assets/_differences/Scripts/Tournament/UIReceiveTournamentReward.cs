using System;

using Airion.DailyRewards;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;

using Zenject;

public class UIReceiveTournamentReward : MonoBehaviour {
    public event Action<int> Received;
    
    [SerializeField] TextMeshProUGUI _rewardedAmountText = default;
    [SerializeField] UIView _view = default;

    [Inject] Tournament _tournament = default;
    
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
        _tournament.HandleExit();
    }
}
