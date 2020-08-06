using System;
using System.Collections;
using System.Collections.Generic;

using Airion.Currency;

using DG.Tweening;

using UnityEngine;

using Zenject;

public class UIEnergyBarUpdater : MonoBehaviour {
    [SerializeField] RectTransform _bar = default;
    [SerializeField] Vector3 _borderPos = default;
    [SerializeField] int _maxCurrencyAmount = default;
    // [SerializeField] GameObject _plusButton = default;

    [Inject] CurrencyManager _currencyManager = default;

    Vector3 _startRectPos;
    Currency _currency;

    const string CURRENCY_NAME = "Energy";

    void Start() {
        _currency = _currencyManager.GetCurrency(CURRENCY_NAME);
        _currency.Updated += CurrencyOnUpdated;

        _startRectPos = _bar.anchoredPosition;
        CurrencyOnUpdated(0);
    }

    void OnDestroy() {
        _currency.Updated -= CurrencyOnUpdated;
    }

    void CurrencyOnUpdated(float obj) {
        var relativeCurrencyAmount = _currency.Amount / _maxCurrencyAmount;
        SetBarPos(relativeCurrencyAmount);
        // _plusButton.SetActive(_currency.Amount < _maxCurrencyAmount);
    }

    void SetBarPos(float percent) {
        DOTween.Kill(this);
        var newPos = Vector3.Lerp(_borderPos, _startRectPos, percent);
        _bar.DOAnchorPos(newPos, 0.25f).SetId(this);
    }
}