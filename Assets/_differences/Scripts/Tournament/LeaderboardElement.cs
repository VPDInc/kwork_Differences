using System.Linq;

using Facebook.Unity;

using PlayFab;
using PlayFab.ClientModels;

using Sirenix.Utilities;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class LeaderboardElement : MonoBehaviour {
    public LeaderboardPlayer Player { get; private set; }
    
    [SerializeField] TextMeshProUGUI _positionText = default;
    [SerializeField] Image _avatar = default;
    [SerializeField] TextMeshProUGUI _displayName = default;
    [SerializeField] TextMeshProUGUI _score = default;
    [SerializeField] TextMeshProUGUI _reward = default;
    [SerializeField] Image _back = default;
    [SerializeField] Color _backColorIfMe = Color.magenta;

    int _index;

    [Inject] TournamentRewards _tournamentRewards = default;
    
    public void Fill(int placeInLeaderboard, int placeInGlobal, LeaderboardPlayer player) {
        _positionText.text = (placeInLeaderboard+1).ToString();
        _avatar.sprite = null;
        _displayName.text = player.DisplayName;
        _score.text = player.Score.ToString();
        Player = player;
        _reward.text = _tournamentRewards.GetRewardByPlace(placeInGlobal).ToString();
        if (player.IsMe) {
            _back.color = _backColorIfMe;
        }
    }

    public void SetIcon(Sprite icon) {
        _avatar.sprite = icon;
    }

    public void SetPosition(int pos) {
        _positionText.text = (pos+1).ToString();
    }
}
