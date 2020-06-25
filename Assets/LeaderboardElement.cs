using Sirenix.Utilities;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class LeaderboardElement : MonoBehaviour {
    [SerializeField] TextMeshProUGUI _positionText = default;
    [SerializeField] Image _avatar = default;
    [SerializeField] TextMeshProUGUI _displayName = default;
    [SerializeField] TextMeshProUGUI _score = default;
    [SerializeField] TextMeshProUGUI _reward = default;

    public void Fill(int index, LeaderboardPlayer player) {
        _positionText.text = index.ToString();
        _avatar.sprite = null;
        _displayName.text = player.DisplayName.IsNullOrWhitespace() ? player.Id : player.DisplayName;
        _score.text = player.Score.ToString();
        _reward.text = "0";
    }
}
