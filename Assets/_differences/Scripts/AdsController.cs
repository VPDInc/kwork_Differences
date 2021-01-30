using Airion.Currency;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;

using Zenject;

public class AdsController : MonoBehaviour {

    const float MAX_VOLUME = 0;

    [SerializeField] int _adsCoinReward = 50;
    [SerializeField] string _titleText = "CONGRADULATIONS!";
    [SerializeField] string _messageText = "Your reward:";
    [SerializeField] string _coinsRewardPrefix = "<sprite=0> ";

    [Inject] Advertisement _advertisement = default;
    [Inject] AdsPopup _adsPopup = default;

    public void RequestAd() {

        _advertisement.ShowRewardedVideo(successCallback:ReceiveReward);
    }

    void ReceiveReward() {
        _advertisement.ChangeVolume(MAX_VOLUME);

        _adsPopup.Open(_titleText, _messageText, _adsCoinReward,  _coinsRewardPrefix + _adsCoinReward);
        Analytic.CurrencyEarn(_adsCoinReward, "ads-watched", "");
    }
}
