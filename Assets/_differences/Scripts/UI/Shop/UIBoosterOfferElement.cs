using System.Collections;
using System.Collections.Generic;

using Airion.Currency;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIBoosterOfferElement : MonoBehaviour {
    [SerializeField] TMP_Text _titleLabel = default;
    [SerializeField] Image _icon = default;
    [SerializeField] TMP_Text _amountLabel = default;
    [SerializeField] TMP_Text _costLabel = default;

    [Inject] CurrencyManager _currencyManager = default;
    [Inject] LevelController _levelController = default;

    Currency _coinsCurrency;
    Currency _currency;
    int _amountToBuy;
    int _cost;

    const string COINS_CURRENCY_ID = "Soft";

    void Start() {
        _coinsCurrency = _currencyManager.GetCurrency(COINS_CURRENCY_ID);
    }

    public void Setup(Currency currency, string title, Sprite icon, int amount, int cost) {
        _currency = currency;
        _amountToBuy = amount;
        _cost = cost;
        
        _titleLabel.text = title;
        _icon.sprite = icon;
        _amountLabel.text = amount.ToString();
        _costLabel.text = cost + " <sprite=0>";
    }

    public void Buy() {
        if (_coinsCurrency.IsEnough(_cost)) {
            _coinsCurrency.Spend(_cost);
            Analytic.CurrencySpend(_cost, "booster-bought", _currency.name, _levelController.LastLevelNum);
            _currency.Earn(_amountToBuy);
        } else {
            Debug.Log("Not enough coins");
        }
    }
}