using System;
using System.Collections.Generic;
using System.Linq;

using Airion.Currency;
using Airion.Extensions;

using Doozy.Engine.UI;

using Facebook.Unity;

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
    [Inject] PlayerInfoController _infoController = default;
    [Inject] DiContainer _container = default;
    [Inject] UIReceiveTournamentReward _receiveReward = default;
    [Inject] TournamentRewards _rewards = default;
    [Inject] CurrencyManager _currencyManager = default;

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
                var reward = _rewards.GetRewardByPlace(index);
                if (reward.Length > 0) {
                    _receiveReward.Show(reward, index);
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
                    _currencyManager.GetCurrency("Aim").Earn(rewardInfo.Amount);
                    break;
                case RewardEnum.Clock:
                    _currencyManager.GetCurrency("Watch").Earn(rewardInfo.Amount);
                    break;
                case RewardEnum.Compass:
                    _currencyManager.GetCurrency("Compass").Earn(rewardInfo.Amount);
                    break;
                case RewardEnum.Zoom:
                    _currencyManager.GetCurrency("Zoom").Earn(rewardInfo.Amount);
                    break;
                case RewardEnum.Coin:
                    _currencyManager.GetCurrency("Soft").Earn(rewardInfo.Amount);
                    break;
                case RewardEnum.Energy:
                    _currencyManager.GetCurrency("Energy").Earn(rewardInfo.Amount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        _receiveReward.Hide();
    }

    void SetIcons() {
        if (_winner1.Player != null) {
            if (_winner1.Player.IsMe) {
                _winner1.PlayerIcon.sprite = _infoController.PlayerIcon;
            } else {
                _winner1.PlayerIcon.sprite = _infoController.GetRandomIcon();
                if (!string.IsNullOrWhiteSpace(_winner1.Player.Facebook)) {
                    FB.API($"{_winner1.Player.Facebook}/picture?type=square&height=200&width=200", HttpMethod.GET,
                           res => {
                               _winner1.PlayerIcon.sprite =
                                   Sprite.Create(res.Texture, new Rect(0, 0, 200, 200), new Vector2());
                           });
                }
            }
        }

        if (_winner2.Player != null) {
            if (_winner2.Player.IsMe) {
                _winner2.PlayerIcon.sprite = _infoController.PlayerIcon;
            } else {
                _winner2.PlayerIcon.sprite = _infoController.GetRandomIcon();
                if (!string.IsNullOrWhiteSpace(_winner2.Player.Facebook)) {
                    FB.API($"{_winner2.Player.Facebook}/picture?type=square&height=200&width=200", HttpMethod.GET,
                           res => {
                               _winner2.PlayerIcon.sprite =
                                   Sprite.Create(res.Texture, new Rect(0, 0, 200, 200), new Vector2());
                           });
                }
            }
        }

        if (_winner3.Player != null) {
            if (_winner3.Player.IsMe) {
                _winner3.PlayerIcon.sprite = _infoController.PlayerIcon;
            } else {
                _winner3.PlayerIcon.sprite = _infoController.GetRandomIcon();
                if (!string.IsNullOrWhiteSpace(_winner3.Player.Facebook)) {
                    FB.API($"{_winner3.Player.Facebook}/picture?type=square&height=200&width=200", HttpMethod.GET,
                           res => {
                               _winner3.PlayerIcon.sprite =
                                   Sprite.Create(res.Texture, new Rect(0, 0, 200, 200), new Vector2());
                           });
                }
            }
        }

        foreach (var pair in _leaderboardElements) {
            var element = pair.Value;
            var id = element.Player.Id;
            if (element.Player.IsMe) {
                SetIconTo(id, _infoController.PlayerIcon);
                continue;
            }

            SetIconTo(id, _infoController.GetRandomIcon());

            if (!string.IsNullOrWhiteSpace(element.Player.Facebook)) {
                FB.API($"{element.Player.Facebook}/picture?type=square&height=200&width=200", HttpMethod.GET,
                       res => { SetIconTo(id, Sprite.Create(res.Texture, new Rect(0, 0, 200, 200), new Vector2())); });
            }
        }

        void SetIconTo(string id, Sprite icon) {
            if (_leaderboardElements.ContainsKey(id)) {
                var element = _leaderboardElements[id];
                element.SetIcon(icon);
            }
        }
    }
}