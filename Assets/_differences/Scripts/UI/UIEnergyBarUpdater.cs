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
    [SerializeField] GameObject _energyLabel = default;
    [SerializeField] GameObject _infinityLabel = default;

    [Inject] CurrencyManager _currencyManager = default;
    [Inject] EnergyController _energyController = default;

    Vector3 _startRectPos;
    Currency _currency;
    bool _isInfinity;

    const string CURRENCY_NAME = "Energy";

    void Start() {
        _currency = _currencyManager.GetCurrency(CURRENCY_NAME);
        _currency.Updated += CurrencyOnUpdated;

        _startRectPos = _bar.anchoredPosition;
        CurrencyOnUpdated(0);
    }

    void Update() {
        var isInfinity = _energyController.IsInfinityTimeOn;
        if(isInfinity == _isInfinity) return;
        if (isInfinity) {
            HandleInfinityOn();
        } else {
            HandleInfinityOff();
        }
    }

    void OnDestroy() {
        _currency.Updated -= CurrencyOnUpdated;
    }

    void HandleInfinityOn() {
        _infinityLabel.SetActive(true);
        _energyLabel.SetActive(false);
        SetBarPos(1);
    }
    
    void HandleInfinityOff() {
        _infinityLabel.SetActive(false);
        _energyLabel.SetActive(true);
        CurrencyOnUpdated(0);
    }

    void CurrencyOnUpdated(float obj) {
        if(_isInfinity) return;
        var relativeCurrencyAmount = _currency.Amount / _maxCurrencyAmount;
        SetBarPos(relativeCurrencyAmount);
    }

    void SetBarPos(float percent) {
        DOTween.Kill(this);
        var newPos = Vector3.Lerp(_borderPos, _startRectPos, percent);
        _bar.DOAnchorPos(newPos, 0.25f).SetId(this);
    }
}