using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class UILeaderboardLastWinner : MonoBehaviour {
   public LeaderboardPlayer Player { get; private set; }
   public Image PlayerIcon => _avatar;
   
   [SerializeField] Image _avatar = default;
   [SerializeField] TextMeshProUGUI _displayName = default;
   [SerializeField] TextMeshProUGUI _score = default;
   [SerializeField] TextMeshProUGUI _reward = default;
   
   public void Fill(LeaderboardPlayer player) {
      _avatar.sprite = null;
      _displayName.text = player.DisplayName;
      _score.text = player.Score.ToString();
      _reward.text = "0";
      Player = player;
   }
}
