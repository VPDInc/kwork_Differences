using System;
using System.Linq;

using Airion.Extensions;

using DG.Tweening;

using Facebook.Unity;

using PlayFab;
using PlayFab.ClientModels;

using Sirenix.Utilities;

using TMPro;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

using Zenject;

public class LeaderboardElement : MonoBehaviour {
    public LeaderboardPlayer Player { get; private set; }
    public Sprite Avatar => _avatar.sprite;

    [SerializeField] TextMeshProUGUI _positionText = default;
    [SerializeField] Image _avatar = default;
    [SerializeField] TextMeshProUGUI _displayName = default;
    [SerializeField] TextMeshProUGUI _score = default;
    [SerializeField] GameObject _backIfMe = default;
    [SerializeField] GameObject _backIfNotMe = default;
    [SerializeField] Image _rewardBox = default;
    [SerializeField] Sprite[] _rewardBoxSprites = default;
    [Header("Popup")] [SerializeField] CanvasGroup _popupCanvasGroup = default;
    [SerializeField] Transform _popupRewardHolder = default;
    [SerializeField] PopupRewardElement _popupRewardElementPrefab = default;

    [Inject] TournamentRewards _tournamentRewards = default;
    
    int _index;
    RewardInfo[] _rewardInfos;
    Transform _lastParent;
    bool _isShowed = false;

    void LateUpdate() {
        if (Input.GetMouseButtonDown(0) && _isShowed) {
            HideReward();
        }
    }

    public void Fill(int placeInLeaderboard, int placeInGlobal, LeaderboardPlayer player) {
        _positionText.text = (placeInLeaderboard+1).ToString();
        _avatar.sprite = null;
        _displayName.text = player.DisplayName;
        _score.text = player.Score.ToString();
        Player = player;
        
        if (player.Score > 0)
            _rewardInfos = _tournamentRewards.GetRewardByPlace(placeInGlobal);
        
        _rewardBox.gameObject.SetActive(_rewardInfos != null);
        var clampedPlace = Mathf.Clamp(placeInGlobal, 0, _rewardBoxSprites.Length - 1);
        _rewardBox.sprite = _rewardBoxSprites[clampedPlace];
        
        _backIfMe.SetActive(player.IsMe);
        _backIfNotMe.SetActive(!player.IsMe);
    }

    public void SetIcon(Sprite icon) {
        _avatar.sprite = icon;
    }

    public void SetPosition(int pos) {
        _positionText.text = (pos+1).ToString();
    }

    public void ShowReward() {
        _popupRewardHolder.DestroyAllChildren();
        foreach (RewardInfo rewardInfo in _rewardInfos) {
            var rewardElement = Instantiate(_popupRewardElementPrefab, _popupRewardHolder);
            rewardElement.Setup(rewardInfo.RewardType, rewardInfo.Amount);
        }
        
        _popupCanvasGroup.DOFade(1, 0.25f).OnComplete(() => {
            _isShowed = true;
        });
    }

    public void HideReward() {
        _popupCanvasGroup.DOFade(0, 0.25f);
        _isShowed = false;
    }
}
