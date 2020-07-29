using System;

using Airion.DailyRewards;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIReceiveTournamentReward : MonoBehaviour {
    public event Action<int> Received;

    [SerializeField] Sprite[] _placeBorders = default;
    [SerializeField] Image _borderImage = default;
    [SerializeField] TextMeshProUGUI _rewardedAmountText = default;
    [SerializeField] UIView _view = default;

    [Inject] Tournament _tournament = default;
    
    int _amount;

    public void Show(int amount, int place) {
        _borderImage.sprite = _placeBorders[Mathf.Clamp(place, 0, 3)];
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
