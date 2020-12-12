using System;
using System.Collections.Generic;
using System.Linq;

using Airion.Currency;
using Airion.Extensions;
using Differences;
using Doozy.Engine.UI;

using UnityEngine;

using Zenject;

public class UITournamentEnd : MonoBehaviour {
    [SerializeField] UILeaderboardLastWinner _winner1 = default;
    [SerializeField] UILeaderboardLastWinner _winner2 = default;
    [SerializeField] UILeaderboardLastWinner _winner3 = default;
    [SerializeField] LeaderboardElement _leaderboardElement = default;
    [SerializeField] Transform _content = default;
    [SerializeField] UIView _loadingView = default;

    [Inject] Tournament _tournament = default;
    [Inject] DiContainer _container = default;
    [Inject] UIReceiveTournamentReward _receiveReward = default;
    [Inject] TournamentRewards _rewards = default;
    [Inject] CurrencyManager _currencyManager = default;
    [Inject] AvatarsPool _avatarsPool = default;

    UIView _view;

    readonly Dictionary<string, LeaderboardElement> _leaderboardElements = new Dictionary<string, LeaderboardElement>();
    readonly List<LeaderboardPlayer> _players = new List<LeaderboardPlayer>();

    void Awake() {
        _view = GetComponent<UIView>();
        _loadingView.Show();
    }

    void Start() {
        _tournament.PrevFilled += OnTournamentFilled;
        _tournament.Completed += OnTournamentCompleted;
        _receiveReward.Received += OnRewardReceived;
    }

    void OnDestroy() {
        _tournament.PrevFilled -= OnTournamentFilled;
        _tournament.Completed -= OnTournamentCompleted;
        _receiveReward.Received -= OnRewardReceived;
    }

    [ContextMenu(nameof(OnTournamentCompleted))]
    void OnTournamentCompleted() {
        _view.Show();
        for (var index = 0; index < _players.Count; index++) {
            var player = _players[index];
            if (player.IsMe) {
                if (player.Score > 0) {
                    var reward = _rewards.GetRewardByPlace(index);
                    if (reward.Length > 0) {
                        _receiveReward.Show(reward, index);
                    }
                }
            }
        }
    }

    public void Show() {
        _view.Show();
    }

    public void OnExitClick() {
        _view.Hide();
        _tournament.HandleExit();
    }

    void OnTournamentFilled(LeaderboardPlayer[] players) {
        _leaderboardElements.Clear();
        var orderedPlayers = players.OrderByDescending(player => player.Score).ToArray();
        _players.Clear();
        _players.AddRange(orderedPlayers);
        _content.DestroyAllChildren();

        var placeInLeaderboard = 0;
        var placeInGlobal = 0;

        if (orderedPlayers.Length > 0) {
            var player = orderedPlayers[0];
            _winner1.Fill(0, player);
            CreateElement(player, placeInLeaderboard, placeInGlobal);
        }

        if (orderedPlayers.Length > 1) {
            for (int i = 1; i < orderedPlayers.Length; i++) {
                var player = orderedPlayers[i];

                if (orderedPlayers[i - 1].Score != player.Score) {
                    placeInGlobal++;
                    placeInLeaderboard++;
                }

                if (i == 1)
                    _winner2.Fill(placeInGlobal, player);

                if (i == 2)
                    _winner3.Fill(placeInGlobal, player);
                
                CreateElement(player, placeInLeaderboard, placeInGlobal);
            }
        }

        SetIcons();
        _loadingView.Hide();
    }

    void CreateElement(LeaderboardPlayer player, int placeInLeaderboard, int placeInGlobal) {
        var element = Instantiate(_leaderboardElement, _content);
        _container.InjectGameObject(element.gameObject);
        element.Fill(placeInLeaderboard, placeInGlobal, player);
        _leaderboardElements.Add(player.Id, element);
    }

    void OnRewardReceived(RewardInfo[] rewardInfos) {
        foreach (RewardInfo rewardInfo in rewardInfos) {
            switch (rewardInfo.RewardType) {
                case RewardEnum.Aim:
                    _currencyManager.GetCurrency(CurrencyConstants.AIM).Earn(rewardInfo.Amount);
                    break;
                case RewardEnum.Clock:
                    _currencyManager.GetCurrency(CurrencyConstants.WATCH).Earn(rewardInfo.Amount);
                    break;
                case RewardEnum.Compass:
                    _currencyManager.GetCurrency(CurrencyConstants.COMPASS).Earn(rewardInfo.Amount);
                    break;
                case RewardEnum.Zoom:
                    _currencyManager.GetCurrency(CurrencyConstants.ZOOM).Earn(rewardInfo.Amount);
                    break;
                case RewardEnum.Soft:
                    _currencyManager.GetCurrency(CurrencyConstants.SOFT).Earn(rewardInfo.Amount);
                    Analytic.CurrencyEarn(rewardInfo.Amount, "tournament-won", "");
                    break;
                case RewardEnum.Energy:
                    _currencyManager.GetCurrency(CurrencyConstants.ENERGY).Earn(rewardInfo.Amount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        _receiveReward.Hide();
    }

    void SetIcons() {
        if (_winner1.Player != null) {
            _avatarsPool.SetAvatarAsync(_winner1.Player, (sprite) => { _winner1.PlayerIcon.sprite = sprite;});
        }

        if (_winner2.Player != null) {
            _avatarsPool.SetAvatarAsync(_winner2.Player, (sprite) => { _winner2.PlayerIcon.sprite = sprite;});
        }

        if (_winner3.Player != null) {
            _avatarsPool.SetAvatarAsync(_winner3.Player, (sprite) => { _winner3.PlayerIcon.sprite = sprite;});
        }

        foreach (var pair in _leaderboardElements) {
            var element = pair.Value;
            _avatarsPool.SetAvatarAsync(element.Player, element.SetIcon);
        }
    }
}