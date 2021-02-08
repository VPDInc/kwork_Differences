using System;
using System.Collections;
using System.Collections.Generic;
using Airion.Currency;
using DG.Tweening;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;
using Zenject;

public class AdsPopup : MonoBehaviour {
    [SerializeField] TMP_Text _titleLabel = default;
    [SerializeField] TMP_Text _descriptionLabel = default;
    [SerializeField] TMP_Text _coinLabel = default;
    [SerializeField] CollectFx collectFx = default;

    UIView _view;

    [Inject] CurrencyManager _currencyManager = default;
    Currency _currency = default;
    private float amount = default;
    

    void Awake() {
        _view = GetComponent<UIView>();

        _currency = _currencyManager.GetCurrency(Differences.CurrencyConstants.SOFT);
    }

    public void Open(string title, string description, float coins, string coinsAmount) {
        _view.Show();

        amount = coins;

        _titleLabel.text = title;
        _descriptionLabel.text = description;
        _coinLabel.text = coinsAmount;
    }

    public void Close(float delay) {

        collectFx.SetupTrailEffect(delegate {
            _currency.Earn(amount);
        });
      
        var seq = DOTween.Sequence();
        seq.AppendInterval(delay);
        seq.AppendCallback(() => {
                               _view.Hide();
                           });
    }
}
