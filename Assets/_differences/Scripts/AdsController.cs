using Airion.Currency;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;

using Zenject;

public class AdsController : MonoBehaviour {
    [SerializeField] int _adsCoinReward = 50;
    [Header("PopUp"), SerializeField] string _popupName = "AdsPopup";
    [SerializeField] string _titleText = "CONGRADULATIONS!";
    [SerializeField] string _messageText = "Your reward:";
    [SerializeField] string _coinsRewardPrefix = "<sprite=0> ";

    [Inject] CurrencyManager _currencyManager = default;

    Currency _currency = default;

    const string CURRENCY_NAME = "Soft";
    
    void Start() {
        _currency = _currencyManager.GetCurrency(CURRENCY_NAME);
    }

    public void RequestAd() {
        //TODO: Add ads logic
        RecieveReward();
    }

    void RecieveReward() {
        var popup = UIPopup.GetPopup(_popupName);
        popup.Data.Labels[0].GetComponent<TMP_Text>().text = _titleText;
        popup.Data.Labels[1].GetComponent<TMP_Text>().text = _messageText;
        popup.Data.Labels[2].GetComponent<TMP_Text>().text = _coinsRewardPrefix + _adsCoinReward;
        popup.Show();
        
        
        _currency.Earn(_adsCoinReward);
    }
}
