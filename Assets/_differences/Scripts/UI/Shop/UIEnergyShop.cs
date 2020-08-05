using Airion.Currency;

using UnityEngine;

using Zenject;

public class UIEnergyShop : MonoBehaviour {
    [Header("Pack1")]
    [SerializeField] string _titlePack1 = "Espresso";
    [SerializeField] string _descriptionPack1 = "Espresso";
    [SerializeField] int _costEnergyPack1 = 300;
    [SerializeField] int _energyAmountPack1 = 10;
    [SerializeField] UIOfferElement _offerElementPack1 = default;
    
    [Header("Pack2")]
    [SerializeField] string _titlePack2 = "Cappuccino";
    [SerializeField] string _descriptionPack2 = "Cappuccino";
    [SerializeField] int _costEnergyPack2 = 850;
    [SerializeField] int _energyAmountPack2 = 30;
    [SerializeField] UIOfferElement _offerElementPack2 = default;
    
    [Header("Pack3")]
    [SerializeField] string _titlePack3 = "Macchiato";
    [SerializeField] string _descriptionPack3 = "Macchiato";
    [SerializeField] int _costInfinityEnergy = 3000;
    [SerializeField] UIOfferElement _offerElementPack3 = default;
    
    [Inject] CurrencyManager _currencyManager = default;
    [Inject] EnergyController _energyController = default;

    Currency _energyCurrency;    
    Currency _softCurrency;

    const string ENERGY_CURRENCY_ID = "Energy";
    const string SOFT_CURRENCY_ID = "Soft";

    void Start() {
        _energyCurrency = _currencyManager.GetCurrency(ENERGY_CURRENCY_ID);
        _softCurrency = _currencyManager.GetCurrency(SOFT_CURRENCY_ID);
        
        SetupOffers();
    }

    public void BuyEnergyPack1() {
        if (_softCurrency.IsEnough(_costEnergyPack1)) {
            _softCurrency.Spend(_costEnergyPack1);
            _energyCurrency.Earn(_energyAmountPack1);
            Analytic.CurrencySpend(_costEnergyPack1, "energy-bought", "energy-pack-1");
            Analytic.EnergyEarn(_energyAmountPack1, "energy-bought", "energy-pack-1");
        }
    }
    
    public void BuyEnergyPack2() {
        if (_softCurrency.IsEnough(_costEnergyPack2)) {
            _softCurrency.Spend(_costEnergyPack2);
            _energyCurrency.Earn(_energyAmountPack2);
            Analytic.CurrencySpend(_costEnergyPack2, "energy-bought", "energy-pack-2");
            Analytic.EnergyEarn(_energyAmountPack2, "energy-bought", "energy-pack-2");
        }
    }

    public void BuyUnlimitedEnergy() {
        if (_softCurrency.IsEnough(_costInfinityEnergy)) {
            _softCurrency.Spend(_costInfinityEnergy);
            _energyController.AddInfinityTime();
            Analytic.CurrencySpend(_costEnergyPack2, "energy-bought", "unlimited-pack");
            Analytic.EnergyEarn(_energyAmountPack2, "energy-bought", "unlimited-pack");
        }
    }

    void SetupOffers() {
        _offerElementPack1.Setup(_titlePack1, _descriptionPack1, _energyAmountPack1 + " <sprite=0>", _costEnergyPack1+ " <sprite=0>");
        _offerElementPack2.Setup(_titlePack2, _descriptionPack2, _energyAmountPack2 + " <sprite=0>", _costEnergyPack2+ " <sprite=0>");
        _offerElementPack3.Setup(_titlePack3, _descriptionPack3, "Unlimited <sprite=0> for 1 hour", _costInfinityEnergy+ " <sprite=0>");
    }
}