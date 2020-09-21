using System;
using System.Collections.Generic;
using System.Linq;

using Airion.Extensions;

using Doozy.Engine.UI;

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
    [SerializeField] UIView _helpView = default;
    [SerializeField] RectTransform _viewport = default;
    [SerializeField] RectTransform _arrow = default;
    
    [Inject] Tournament _tournament = default;
    [Inject] DiContainer _container = default;
    [Inject] UITournamentEnd _endTournament = default;
    [Inject] AvatarsPool _avatarPool = default;
    
    UIView _view;
    float _lastUpdateTimestamp = 0;
    int _myPosition = 0;
    int _fullElementsAmount = 0;
    
    RectTransform _my;
    
    bool IsMeInsideView => RectTransformUtility.RectangleContainsScreenPoint(_viewport, _my.transform.position);

    // var position = _scroll.normalizedPosition.y;
    // var step = (1 / (float) _fullElementsAmount);
    // var validOffset = step * (ELEMENTS_COUNT_IN_ONE_SCREEN);
    // // var isPlayerInsideScreen = MyPositionInScrollView <= position && position <= MyPositionInScrollView + validOffset;
    // var isPlayerInsideScreen = position <= MyPositionInScrollView && position <=  MyPositionInScrollView + validOffset;
    //
    // Debug.Log("POs: " + position + " inside " + isPlayerInsideScreen + " mypos " + MyPositionInScrollView + " offset " + validOffset);
    // return isPlayerInsideScreen;
    
    
    float MyPositionInScrollView => 1 - (_myPosition / (float) _fullElementsAmount );

    const float UPDATE_TIMER_EVERY_SECONDS = 60;
    const float ELEMENTS_COUNT_IN_ONE_SCREEN = 7;
    
    readonly Dictionary<string, LeaderboardElement> _leaderboardElements = new Dictionary<string,LeaderboardElement>();
    
    void Awake() {
        _view = GetComponent<UIView>();
        _loadingView.Show();
    }

    void Start() {
        _tournament.CurrentFilled += OnTournamentCurrentFilled;
        _tournament.PrevFilled += OnLastWinnersFilled;
    }

    void OnDestroy() {
        _tournament.CurrentFilled -= OnTournamentCurrentFilled;
        _tournament.PrevFilled -= OnLastWinnersFilled;
    }
    
    void Update() {
        if (Time.time - _lastUpdateTimestamp >= UPDATE_TIMER_EVERY_SECONDS) {
            UpdateTimer();
        }

        if (Input.GetMouseButtonDown(0)) {
            if (_helpView.IsVisible && !_helpView.IsShowing) {
                _helpView.Hide();
            }
        }
    }

    void OnTournamentCurrentFilled(LeaderboardPlayer[] players) {
        Fill(players);
    }

    void Fill(LeaderboardPlayer[] players, bool friendsOnly = false) {
        UpdateTimer();
        
        _leaderboardElements.Clear();
        _content.DestroyAllChildren();
        _fullElementsAmount = 0;
        _myPosition = 0;
        
        var orderedPlayers = players.OrderByDescending(player => player.Score).ToArray();
        var placeInLeaderboard = 0;
        var placeInGlobal = 0;
        
        if (orderedPlayers.Length > 0)
            CreateElement(orderedPlayers[0], placeInLeaderboard, placeInGlobal);

        if (orderedPlayers.Length > 1) {
            for (int i = 1; i < orderedPlayers.Length; i++) {
                var player = orderedPlayers[i];
                if (_leaderboardElements.ContainsKey(player.Id)) {
                    Debug.LogError($"[{GetType()}] already contains element with id {player.Id}", _leaderboardElements[player.Id]);
                } else {
                    if (orderedPlayers[i - 1].Score != player.Score) {
                        placeInGlobal++;
                        placeInLeaderboard++;
                    } 
                    
                    CreateElement(player, placeInLeaderboard, placeInGlobal);
                }

                if (player.IsMe) {
                    _myPosition = placeInLeaderboard;
                }
                
            }
        }
        
        _loadingView.Hide();
        SelectMy();
        SetIcons();
        
        _toLeadersButton.SetActive(IsMeInsideView);
        _toMyButton.SetActive(!IsMeInsideView);
    }

    void CreateElement(LeaderboardPlayer player, int placeInLeaderboard, int placeInGlobal) {
        var element = Instantiate(_leaderboardElement, _content);
        _container.InjectGameObject(element.gameObject);
        _fullElementsAmount++;
        element.Fill(placeInLeaderboard, placeInGlobal, player);
        _leaderboardElements.Add(player.Id, element);
        if (player.IsMe)
            _my = element.GetComponent<RectTransform>();
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

    public void OnOpenTournamentEndClick() {
        _endTournament.Show();
    }

    void SelectMy() {
        var step = (1 / (float) _fullElementsAmount);
        _scroll.verticalNormalizedPosition = Mathf.Clamp01(MyPositionInScrollView + (step * ELEMENTS_COUNT_IN_ONE_SCREEN));
    }

    public void OnScrollChanged(Vector2 scroll) {
        _toLeadersButton.SetActive(IsMeInsideView);
        _toMyButton.SetActive(!IsMeInsideView);
        if (!IsMeInsideView) {
            if (MyPositionInScrollView < _scroll.normalizedPosition.y) {
                _arrow.transform.rotation = Quaternion.Euler(0,0,0);
            } else {
                _arrow.transform.rotation = Quaternion.Euler(0,0,180);
            }
        }
    }

    public void OnFriendsOnlyToggleSwitch(bool isFriendsOnly) {
        Filter(isFriendsOnly);
    }

    void Filter(bool isFriendsOnly) {
        var elements = _leaderboardElements.Values;
        var ordered = elements.OrderByDescending(e => e.Player.Score).ToArray();

        for (int i = 0; i < ordered.Length; i++) {
            var element = ordered[i];
            var isFriend = (element.Player.IsFriend || element.Player.IsMe);
            if (isFriend && isFriendsOnly || !isFriendsOnly) {
                element.gameObject.SetActive(true);
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
                _avatarPool.SetAvatarAsync(winner, (sprite) => {
                    _winnerAvatar1.sprite = sprite;
                });
            }
            
            if (i == 1) {      
                _avatarPool.SetAvatarAsync(winner, (sprite) => {
                    _winnerAvatar2.sprite = sprite;
                });
            }
            
            if (i == 2) {
                _avatarPool.SetAvatarAsync(winner, (sprite) => {
                    _winnerAvatar3.sprite = sprite;
                });
            }
        }
    }

    void SetIcons() {
        foreach (var pair in _leaderboardElements) {
            var element = pair.Value;
            _avatarPool.SetAvatarAsync(element.Player, element.SetIcon);
        }
    }
}
