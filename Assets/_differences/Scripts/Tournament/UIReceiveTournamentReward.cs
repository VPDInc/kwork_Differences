using System;

using Airion.DailyRewards;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIReceiveTournamentReward : MonoBehaviour {
    public event Action<RewardInfo[]> Received;

    [SerializeField] Sprite[] _placeBorders = default;
    [SerializeField] Image _borderImage = default;
    [SerializeField] UIView _view = default;
    [SerializeField] PopupRewardElement _rewardElementPrefab = default;
    [SerializeField] Transform _rewardHolder = default;
    [SerializeField] TMP_Text _placeText = default;
    [SerializeField] ParticleSystem _particles = default;

    [Inject] Tournament _tournament = default;

    RewardInfo[] _rewardInfos;

    public void Show(RewardInfo[] rewardInfos, int place) {
        _rewardInfos = rewardInfos;
        _borderImage.sprite = _placeBorders[Mathf.Clamp(place, 0, 3)];
        _placeText.text = (place + 1) + " place";
        foreach (RewardInfo rewardInfo in rewardInfos) {
            var rewardElement = Instantiate(_rewardElementPrefab, _rewardHolder);
            rewardElement.Setup(rewardInfo.RewardType, rewardInfo.Amount);
        }
        _view.Show();
        _particles.Play();
    }

    public void Hide() {
        _view.Hide();
    }

    public void OnReceiveClick() {
        Received?.Invoke(_rewardInfos);
        _tournament.HandleExit();
    }
}
