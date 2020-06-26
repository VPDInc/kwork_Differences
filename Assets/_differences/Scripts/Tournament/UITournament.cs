using System;
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
    [SerializeField] TextMeshProUGUI _buttonScore = default;

    [Inject] Tournament _tournament = default;
    
    UIView _view;
    float _lastUpdateTimestamp = 0;

    const float UPDATE_TIMER_EVERY_SECONDS = 60;
    
    void Awake() {
        _view = GetComponent<UIView>();
        _loadingView.Show();
    }

    void Start() {
        _tournament.Filled += OnTournamentFilled;
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
        
        _content.DestroyAllChildren();
        
        if (friendsOnly)
            players = players.Where(p => p.IsFriend).ToArray();
        var orderedPlayers = players.OrderByDescending(player => player.Score).ToArray();
        for (int i = 0; i < orderedPlayers.Length; i++) {
            var player = orderedPlayers[i];
            var element = Instantiate(_leaderboardElement, _content);
            element.Fill(i, player);
        }
        _loadingView.Hide();
    }

    void UpdateTimer() {
        _tournamentDuration.text = (_tournament.NextReset - DateTime.Now).ToString(@"d\d\ hh\h\ mm\m");
        _lastUpdateTimestamp = Time.time;
    }

    void OnDestroy() {
        _tournament.Filled -= OnTournamentFilled;
    }

    public void OnShowClick() {
        _view.Show();
    }
    
    public void OnExitClick() {
        _view.Hide();
    }

    public void OnPositionButtonClick() {
        
    }

    public void OnFriendsOnlyToggleSwitch(bool isFriendsOnly) {
        Fill(_tournament.CurrentPlayers, isFriendsOnly);
    }
}
