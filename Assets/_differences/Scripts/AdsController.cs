using UnityEngine;
using Zenject;

public class AdsController : MonoBehaviour
{
    private const float MAX_VOLUME = 0;

    [Inject] private readonly AdsPopup _adsPopup = default;
    [Inject] private readonly Advertisement _advertisement = default;

    [SerializeField] private int _adsCoinReward = 50;
    [SerializeField] private string _coinsRewardPrefix = "<sprite=0> ";
    [SerializeField] private string _messageText = "Your reward:";
    [SerializeField] private string _titleText = "CONGRATULATIONS!";

    public void RequestAd()
    {
        _advertisement.ShowRewardedVideo(ReceiveReward, FailReward, closedCallback:OnCloseReward);
    }

    private void ReceiveReward()
    {
        _advertisement.ChangeVolume(MAX_VOLUME);

        _adsPopup.Open(_titleText, _messageText, _adsCoinReward, _coinsRewardPrefix + _adsCoinReward);
        Analytic.CurrencyEarn(_adsCoinReward, "ads-watched", string.Empty);
    }

    private void FailReward()
    {
        _advertisement.ChangeVolume(MAX_VOLUME);
    }

    private void OnCloseReward()
    {
        _advertisement.ChangeVolume(MAX_VOLUME);
    }
}