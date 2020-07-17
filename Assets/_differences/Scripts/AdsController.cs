using Airion.Currency;

using UnityEngine;

using Zenject;

public class AdsController : MonoBehaviour {
    [SerializeField] int _adsCoinReward = 50;

    [Inject] CurrencyManager _currencyManager = default;

    Currency _currency = default;

    const string CURRENCY_NAME = "Soft";
    
    void Start() {
        _currencyManager.GetCurrency(CURRENCY_NAME);
    }

    public void RequestAd() {
        //TODO: Add ads logic
        RecieveReward();
    }

    void RecieveReward() {
        _currency.Earn(_adsCoinReward);
    }
}
