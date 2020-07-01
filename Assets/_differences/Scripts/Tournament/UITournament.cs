using System;
using System.Collections.Generic;
using System.Linq;

using Airion.Extensions;

using Doozy.Engine.UI;

using Facebook.Unity;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UITournament : MonoBehaviour {
    [SerializeField] Image _winnerAvatar1 = default;
    [SerializeField] Image _winnerAvatar2 = default;
    [SerializeField] Image _winnerAvatar3 = default;
    [SerializeField] LeaderboardElement _leaderboardElement = default;
    [SerializeField] TextMeshProUGUI _tournamentDuration = default;
    [SerializeField] Transform _content = default;
    [SerializeField] UIView _loadingView = default;
    [SerializeField] ScrollRect _scroll = default;
    [SerializeField] GameObject _toMyButton = default;
    [SerializeField] GameObject _toLeadersButton = default;

    [Inject] Tournament _tournament = default;
    [Inject] DiContainer _container = default;
    [Inject] PlayerInfoController _infoController = default;
    
    UIView _view;
    float _lastUpdateTimestamp = 0;
    int _myPosition = 0;
    int _fullAmount = 0;
    bool IsMeInsideView {
        get {
            var position = _scroll.normalizedPosition.y;
            var step = (1 / (float) _fullAmount);
            var validOffset = step * (ELEMENTS_COUNT_IN_ONE_SCREEN);
            var isPlayerInsideScreen = MyPositionInScrollView <= position && position <= MyPositionInScrollView + validOffset;
            return isPlayerInsideScreen;
        }
    }
    
    float MyPositionInScrollView => 1 - (_myPosition / (float) _fullAmount);

    const float UPDATE_TIMER_EVERY_SECONDS = 60;
    const float ELEMENTS_COUNT_IN_ONE_SCREEN = 6;
    
    readonly Dictionary<string, LeaderboardElement> _leaderboardElements = new Dictionary<string,LeaderboardElement>();
    
    void Awake() {
        _view = GetComponent<UIView>();
        _loadingView.Show();
    }

    void Start() {
        _tournament.Filled += OnTournamentFilled;
        _tournament.FilledLastWinners += OnLastWinnersFilled;
    }

    void OnDestroy() {
        _tournament.Filled -= OnTournamentFilled;
        _tournament.FilledLastWinners -= OnLastWinnersFilled;
    }
    
    void Update() {
        if (Time.time - _lastUpdateTimestamp >= UPDATE_TIMER_EVERY_SECONDS) {
            UpdateTimer();
        }
    }

    void OnTournamentFilled(LeaderboardPlayer[] players) {
        Fill(players);
    }

    void Fill(LeaderboardPlayer[] players, bool friendsOnly = false) {
        UpdateTimer();
        
        _leaderboardElements.Clear();
        _content.DestroyAllChildren();
        _fullAmount = 0;
        _myPosition = 0;
        
        var orderedPlayers = players.OrderByDescending(player => player.Score).ToArray();
        var current = 0;
        for (int i = 0; i < orderedPlayers.Length; i++) {
            var player = orderedPlayers[i];
            if (friendsOnly && player.IsFriend || !friendsOnly) {
                var element = Instantiate(_leaderboardElement, _content);
                _container.InjectGameObject(element.gameObject);
                _fullAmount++;
                element.Fill(current, i, player);
                _leaderboardElements.Add(player.Id, element);
                current++;
            }
            
            if (player.IsMe)
                _myPosition = current;
            
        }
        _loadingView.Hide();
        SelectMy();
        SetIcons();
        
        _toLeadersButton.SetActive(IsMeInsideView);
        _toMyButton.SetActive(!IsMeInsideView);
    }

    void UpdateTimer() {
        var delta = (_tournament.NextReset - DateTime.Now);
        var toStr = delta;
        if (delta < TimeSpan.Zero) {
            toStr = TimeSpan.Zero;
            _tournament.TryReloadTimed();
        }
        
        _tournamentDuration.text = toStr.ToString(@"d\d\ hh\h\ mm\m");
        _lastUpdateTimestamp = Time.time;
    }

    public void OnShowClick() {
        _view.Show();
    }
    
    public void OnExitClick() {
        _view.Hide();
    }

    public void OnPositionButtonClick() {
        if (IsMeInsideView) {
            _scroll.verticalNormalizedPosition = 1;
        } else {
           SelectMy();
        }
    }

    void SelectMy() {
        var step = (1 / (float) _fullAmount);
        _scroll.verticalNormalizedPosition = Mathf.Clamp01(MyPositionInScrollView + (step * ELEMENTS_COUNT_IN_ONE_SCREEN));
    }

    public void OnScrollChanged(Vector2 scroll) {
        _toLeadersButton.SetActive(IsMeInsideView);
        _toMyButton.SetActive(!IsMeInsideView);
    }

    public void OnFriendsOnlyToggleSwitch(bool isFriendsOnly) {
        Filter(isFriendsOnly);
    }

    void Filter(bool isFriendsOnly) {
        var elements = _leaderboardElements.Values;
        var ordered = elements.OrderByDescending(e => e.Player.Score).ToArray();
        var pos = 0;
        for (int i = 0; i < ordered.Length; i++) {
            var element = ordered[i];
            if (element.Player.IsFriend && isFriendsOnly || !isFriendsOnly) {
                element.gameObject.SetActive(true);
                element.SetPosition(pos);
                pos++;
            } else {
                element.gameObject.SetActive(false);
            }
        }
    }
    
    void OnLastWinnersFilled(LeaderboardPlayer[] winners) {
        var ordered = winners.OrderByDescending(p => p.Score).ToArray();
        for (int i = 0; i < ordered.Length; i++) {
            if (i >= ordered.Length)
                return;
            var winner = ordered[i];
            if (i == 0) {
                _winnerAvatar1.sprite = _infoController.GetRandomIcon();
                if (!string.IsNullOrWhiteSpace(winner.Facebook)) {
                    FB.API($"{winner.Facebook}/picture?type=square&height=200&width=200", HttpMethod.GET,
                        res => {
                            _winnerAvatar1.sprite = Sprite.Create(res.Texture, new Rect(0, 0, 200, 200), new Vector2());
                        });
                } 
            }
            
            if (i == 1) {
                _winnerAvatar1.sprite = _infoController.GetRandomIcon();
                if (!string.IsNullOrWhiteSpace(winner.Facebook)) {
                    FB.API($"{winner.Facebook}/picture?type=square&height=200&width=200", HttpMethod.GET,
                        res => {
                            _winnerAvatar2.sprite = Sprite.Create(res.Texture, new Rect(0, 0, 200, 200), new Vector2());
                        });
                } 
            }
            
            if (i == 2) {
                _winnerAvatar1.sprite = _infoController.GetRandomIcon();
                if (!string.IsNullOrWhiteSpace(winner.Facebook)) {
                    FB.API($"{winner.Facebook}/picture?type=square&height=200&width=200", HttpMethod.GET,
                        res => {
                            _winnerAvatar3.sprite = Sprite.Create(res.Texture, new Rect(0, 0, 200, 200), new Vector2());
                        });
                } 
            }
        }
    }

    void SetIcons() {
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
                    res => {
                        SetIconTo(id, Sprite.Create(res.Texture, new Rect(0, 0, 200, 200), new Vector2()));
                    });
            } 
        }
    }

    void SetIconTo(string id, Sprite icon) {
        if (_leaderboardElements.ContainsKey(id)) {
            var element = _leaderboardElements[id];
            element.SetIcon(icon);
        }
    }
}
