using System.Linq;

using Airion.Extensions;

using Doozy.Engine.UI;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UITournamentEnd : MonoBehaviour {
    [SerializeField] Image _winnerAvatar1 = default;
    [SerializeField] Image _winnerAvatar2 = default;
    [SerializeField] Image _winnerAvatar3 = default;
    [SerializeField] LeaderboardElement _leaderboardElement = default;
    [SerializeField] Transform _content = default;

    UIView _view;
    
    [Inject] Tournament _tournament = default;
    
    void Awake() {
        _view = GetComponent<UIView>();
    }

    void Start() {
        _tournament.Completed += OnTournamentFilled;
    }
    
    void OnDestroy() {
        _tournament.Filled -= OnTournamentFilled;
    }
    
    public void OnExitClick() {
        _view.Hide();
    }

    void OnTournamentFilled(LeaderboardPlayer[] players) {
        var orderedPlayers = players.OrderByDescending(player => player.Score).ToArray();
        _content.DestroyAllChildren();
        for (int i = 0; i < orderedPlayers.Length; i++) {
            var player = orderedPlayers[i];
            var element = Instantiate(_leaderboardElement, _content);
            element.Fill(i, player);
        }
        _view.Show();
    }
}