using Airion.Currency;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;

using Zenject;

public class AdsController : MonoBehaviour {
    [SerializeField] int _adsCoinReward = 50;
    [SerializeField] string _titleText = "CONGRADULATIONS!";
    [SerializeField] string _messageText = "Your reward:";
    [SerializeField] string _coinsRewardPrefix = "<sprite=0> ";

    [Inject] CurrencyManager _currencyManager = default;
    [Inject] Advertisement _advertisement = default;
    [Inject] AdsPopup _adsPopup = default;

    Currency _currency = default;

    void Start() {
        _currency = _currencyManager.GetCurrency(Differences.CurrencyConstants.SOFT);
    }

    public void RequestAd() {
        _advertisement.ShowRewardedVideo(successCallback:ReceiveReward);
    }

    void ReceiveReward() {
        _adsPopup.Open(_titleText, _messageText, _coinsRewardPrefix + _adsCoinReward);
        _currency.Earn(_adsCoinReward);
        Analytic.CurrencyEarn(_adsCoinReward, "ads-watched", "");
    }
}
