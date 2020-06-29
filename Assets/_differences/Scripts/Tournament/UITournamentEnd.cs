using System.Collections.Generic;
using System.Linq;

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

    UIView _view;
    
    [Inject] Tournament _tournament = default;
    [Inject] PlayerInfoController _infoController = default;
    [Inject] DiContainer _container = default;
    
    readonly Dictionary<string, LeaderboardElement> _leaderboardElements = new Dictionary<string,LeaderboardElement>();
    
    void Awake() {
        _view = GetComponent<UIView>();
    }

    void Start() {
        _tournament.Completed += OnTournamentFilled;
    }
    
    void OnDestroy() {
        _tournament.Completed -= OnTournamentFilled;
    }
    
    public void OnExitClick() {
        _view.Hide();
    }

    void OnTournamentFilled(LeaderboardPlayer[] players) {
        _leaderboardElements.Clear();
        var orderedPlayers = players.OrderByDescending(player => player.Score).ToArray();
        _content.DestroyAllChildren();

        for (int i = 0; i < 3; i++) {
            if (i >= orderedPlayers.Length) {
                break;
            }

            var player = orderedPlayers[i];
            
            if (i == 0)
                _winner1.Fill(0,player);
            if (i == 1)
                _winner2.Fill(0, player);
            if (i == 2)
                _winner3.Fill(0, player);
        }

        if (orderedPlayers.Length > 3) {
            for (int i = 3; i < orderedPlayers.Length; i++) {
                var player = orderedPlayers[i];
                var element = Instantiate(_leaderboardElement, _content);
                _container.InjectGameObject(element.gameObject);
                element.Fill(i, player);
                _leaderboardElements.Add(player.Id, element);
            }
        }

        SetIcons();
        _view.Show();
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
                    res => {
                        SetIconTo(id, Sprite.Create(res.Texture, new Rect(0, 0, 200, 200), new Vector2()));
                    });
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