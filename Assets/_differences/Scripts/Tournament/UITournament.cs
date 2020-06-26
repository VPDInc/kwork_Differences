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
    [SerializeField] ScrollRect _scroll = default;
    [SerializeField] GameObject _toMyButton = default;
    [SerializeField] GameObject _toLeadersButton = default;

    [Inject] Tournament _tournament = default;
    [Inject] DiContainer _container = default;
    
    UIView _view;
    float _lastUpdateTimestamp = 0;
    int _myPosition = 0;
    int _fullAmount = 0;
    bool IsMeInsideView {
        get {
            var position = _scroll.normalizedPosition.y;
            var step = (1 / (float) _fullAmount);
            var validOffset = step * (ELEMENTS_COUNT_IN_ONE_SCREEN);
            var isPlayerInsideScreen = position <= MyPositionInScrollView && MyPositionInScrollView <= position + validOffset;
            return isPlayerInsideScreen;
        }
    }
    
    float MyPositionInScrollView => 1 - (_myPosition / (float) _fullAmount);

    const float UPDATE_TIMER_EVERY_SECONDS = 60;
    const float ELEMENTS_COUNT_IN_ONE_SCREEN = 7;
    
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
        _fullAmount = 0;
        _myPosition = 0;
        
        if (friendsOnly)
            players = players.Where(p => p.IsFriend).ToArray();

        var orderedPlayers = players.OrderByDescending(player => player.Score).ToArray();
        for (int i = 0; i < orderedPlayers.Length; i++) {
            var player = orderedPlayers[i];
            var element = Instantiate(_leaderboardElement, _content);
            _container.InjectGameObject(element.gameObject);
            _fullAmount++;
            element.Fill(i, player);
            if (player.IsMe)
                _myPosition = i;
        }
        _loadingView.Hide();
        SelectMy();
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
        if (IsMeInsideView) {
            _scroll.verticalNormalizedPosition = 1;
        } else {
           SelectMy();
        }
    }

    void SelectMy() {
        var step = (1 / (float) _fullAmount);
        _scroll.verticalNormalizedPosition = Mathf.Max(MyPositionInScrollView - (step * ELEMENTS_COUNT_IN_ONE_SCREEN), 0);
    }

    public void OnScrollChanged(Vector2 scroll) {
        _toLeadersButton.SetActive(IsMeInsideView);
        _toMyButton.SetActive(!IsMeInsideView);
    }

    public void OnFriendsOnlyToggleSwitch(bool isFriendsOnly) {
        Fill(_tournament.CurrentPlayers, isFriendsOnly);
    }
}
