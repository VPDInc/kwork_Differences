using Airion.Currency;
using Differences;
using Doozy.Engine;
using Doozy.Engine.UI;
using System;
using UnityEngine;

using Zenject;

public class UIEnergyShop : MonoBehaviour {
    [SerializeField] UITrailEffect _uiTrailEffectPrefab = default;
    [SerializeField] RectTransform _fxTarget = default;
    [SerializeField] float _pauseBetweenSpawns = 0.02f;
    [SerializeField] int _fxAmount = 10;
    
    [Header("Pack1")]
    [SerializeField] int _costEnergyPack1 = 300;
    [SerializeField] int _energyAmountPack1 = 10;
    [SerializeField] UIOfferElement _offerElementPack1 = default;
    
    [Header("Pack2")]
    [SerializeField] int _costEnergyPack2 = 850;
    [SerializeField] int _energyAmountPack2 = 30;
    [SerializeField] UIOfferElement _offerElementPack2 = default;
    
    [Header("Pack3")]
    [SerializeField] int _costInfinityEnergy = 3000;
    [SerializeField] UIOfferElement _offerElementPack3 = default;
    
    [Inject] CurrencyManager _currencyManager = default;
    [Inject] EnergyController _energyController = default;
    [Inject] LevelController _levelController = default;

    Currency _energyCurrency;    
    Currency _softCurrency;

    const string OPEN_STORE_EVENT_ID = "OpenCoinStore";
    const string MAX_ENERGY_POPUP_NAME = "MaxEnergy";

    void Start() {
        _energyCurrency = _currencyManager.GetCurrency(CurrencyConstants.ENERGY);
        _softCurrency = _currencyManager.GetCurrency(CurrencyConstants.SOFT);
        
        SetupOffers();
    }

    public void BuyEnergyPack1() {
        if (_energyController.IsEnergyMaxed) {
            ShowMaxEnergyPopup();
            return;
        }
        
        if (_softCurrency.IsEnough(_costEnergyPack1)) {
            SetupTrailEffect(_offerElementPack1.transform, _fxTarget, delegate {
                _softCurrency.Spend(_costEnergyPack1);
                _energyCurrency.Earn(_energyAmountPack1);
            });
            Analytic.CurrencySpend(_costEnergyPack1, "energy-bought", "energy-pack-1", _levelController.LastLevelNum);
        } else {
            GameEventMessage.SendEvent(OPEN_STORE_EVENT_ID);
        }
    }
    
    public void BuyEnergyPack2() {
        if (_energyController.IsEnergyMaxed) {
            ShowMaxEnergyPopup();
            return;
        }
        
        if (_softCurrency.IsEnough(_costEnergyPack2)) {
            SetupTrailEffect(_offerElementPack2.transform, _fxTarget, delegate {
                _softCurrency.Spend(_costEnergyPack2);
                _energyCurrency.Earn(_energyAmountPack2);
            });
            Analytic.CurrencySpend(_costEnergyPack2, "energy-bought", "energy-pack-2", _levelController.LastLevelNum);
        } else {
            GameEventMessage.SendEvent(OPEN_STORE_EVENT_ID);
        }
    }

    public void BuyUnlimitedEnergy() {
        if (_energyController.IsInfinityTimeOn) {
            ShowMaxEnergyPopup();
            return;
        }
        
        if (_softCurrency.IsEnough(_costInfinityEnergy)) {
        
            SetupTrailEffect(_offerElementPack3.transform, _fxTarget, delegate {
                _softCurrency.Spend(_costInfinityEnergy);
                _energyController.AddInfinityTime();
            });

            Analytic.CurrencySpend(_costEnergyPack2, "energy-bought", "unlimited-pack", _levelController.LastLevelNum);
        } else {
            GameEventMessage.SendEvent(OPEN_STORE_EVENT_ID);
        }
    }
    
    void SetupTrailEffect(Transform startTransform, RectTransform targetTransform, Action onSucsses) {
        for (int i = 0; i < _fxAmount; i++) {
            var fx = Instantiate(_uiTrailEffectPrefab, startTransform);
            fx.Setup(targetTransform.position, _pauseBetweenSpawns * i, onSucsses);
        }
    }

    void ShowMaxEnergyPopup() {
        var popup = UIPopup.GetPopup(MAX_ENERGY_POPUP_NAME);
        popup.Show();
    }

    void SetupOffers() {
        _offerElementPack1.Setup("+ " + _energyAmountPack1, _costEnergyPack1+ " <sprite=0>");
        _offerElementPack2.Setup("+ " + _energyAmountPack2, _costEnergyPack2+ " <sprite=0>");
        _offerElementPack3.Setup("Unlimited <sprite=0> for 1 hour", _costInfinityEnergy+ " <sprite=0>");
    }
}