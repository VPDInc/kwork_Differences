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
    
    public void Fill(int index, LeaderboardPlayer player) {
        _positionText.text = index.ToString();
        _avatar.sprite = null;
        _displayName.text = player.DisplayName;
        _score.text = player.Score.ToString();
        _reward.text = "0";
        Player = player;
        if (player.IsMe) {
            _back.color = _backColorIfMe;
        }
    }

    public void SetIcon(Sprite icon) {
        _avatar.sprite = icon;
    }
}
