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

    UIView _view;
    
    [Inject] Tournament _tournament = default;
    
    void Awake() {
        _view = GetComponent<UIView>();
        _loadingView.Show();
    }

    void Start() {
        _tournament.Filled += OnTournamentFilled;
    }

    void OnTournamentFilled(LeaderboardPlayer[] players) {
        _content.DestroyAllChildren();
        var orderedPlayers = players.OrderByDescending(player => player.Score).ToArray();
        for (int i = 0; i <orderedPlayers.Length; i++) {
            var player = orderedPlayers[i];
            var element = Instantiate(_leaderboardElement, _content);
            element.Fill(i, player);
        }
        _loadingView.Hide();
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
        
    }



}
