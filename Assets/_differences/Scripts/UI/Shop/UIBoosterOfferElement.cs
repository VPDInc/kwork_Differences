using System.Collections;
using System.Collections.Generic;

using Airion.Currency;

using Doozy.Engine;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIBoosterOfferElement : MonoBehaviour {
    [SerializeField] TMP_Text _titleLabel = default;
    [SerializeField] Image _icon = default;
    [SerializeField] TMP_Text _amountLabel = default;
    [SerializeField] TMP_Text _costLabel = default;
    
    [SerializeField] UITrailEffect _uiTrailEffectPrefab = default;
    [SerializeField] float _amountDivider = 5;
    [SerializeField] float _pauseBetweenSpawns = 0.02f;

    [Inject] CurrencyManager _currencyManager = default;
    [Inject] LevelController _levelController = default;

    Transform _fxStartTransform;
    Currency _coinsCurrency;
    Currency _currency;
    int _amountToBuy;
    int _cost;

    const string COINS_CURRENCY_ID = "Soft";
    const string OPEN_STORE_EVENT_ID = "OpenCoinStore";

    void Start() {
        _coinsCurrency = _currencyManager.GetCurrency(COINS_CURRENCY_ID);
    }

    public void Setup(Currency currency, string title, Sprite icon, int amount, int cost, Transform fxStartTransform) {
        _currency = currency;
        _amountToBuy = amount;
        _cost = cost;
        
        _titleLabel.text = title;
        _icon.sprite = icon;
        _amountLabel.text = amount.ToString();
        _costLabel.text = cost + " <sprite=0>";

        _fxStartTransform = fxStartTransform;
    }

    public void Buy() {
        if (_coinsCurrency.IsEnough(_cost)) {
            _coinsCurrency.Spend(_cost);
            Analytic.CurrencySpend(_cost, "booster-bought", _currency.name, _levelController.LastLevelNum);
            _currency.Earn(_amountToBuy);
            SetupTrailEffect(100);
        } else {
            GameEventMessage.SendEvent(OPEN_STORE_EVENT_ID);
        }
    }
    
    void SetupTrailEffect(int coinsAmount) {
        for (int i = 0; i < coinsAmount / _amountDivider; i++) {
            var coinFx = Instantiate(_uiTrailEffectPrefab, _fxStartTransform);
            coinFx.Setup(transform.position, _pauseBetweenSpawns * i);
        }
    }
}