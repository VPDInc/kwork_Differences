using System;

using Airion.DailyRewards;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;

public class UIReceiveReward : MonoBehaviour {
    public event Action Received;
    
    [SerializeField] TextMeshProUGUI _rewardedAmountText = default;
    
    [SerializeField] UIView _view = default;
    
    [SerializeField] UITrailEffect _uiTrailEffectPrefab = default;
    [SerializeField] RectTransform _fxStartTransform = default;
    [SerializeField] RectTransform _fxTargetTransform = default;
    [SerializeField] int _coinsFxAmount = 10;
    [SerializeField] float _pauseBetweenSpawns = 0.02f;

    int _rewardAmount;

    public void Show(Reward reward) {
        _rewardAmount = ((CurrencyReward) reward).Amount;
        _rewardedAmountText.text = ((CurrencyReward)reward).Amount.ToString();
        _view.Show();
    }

    public void Hide() {
        _view.Hide();
    }

    public void OnReceiveClick() {
        SetupTrailEffect(_coinsFxAmount);
        Received?.Invoke();
    }

    void SetupTrailEffect(int coinsAmount) {
        for (int i = 0; i < coinsAmount; i++) {
            var coinFx = Instantiate(_uiTrailEffectPrefab, _fxStartTransform);
            coinFx.Setup(_fxTargetTransform.position, _pauseBetweenSpawns * i);
        }
    }
}
