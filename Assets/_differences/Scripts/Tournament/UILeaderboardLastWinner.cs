using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UILeaderboardLastWinner : MonoBehaviour {
   public LeaderboardPlayer Player { get; private set; }
   public Image PlayerIcon => _avatar;
   
   [SerializeField] Image _avatar = default;
   [SerializeField] TextMeshProUGUI _score = default;

   public void Fill(int place, LeaderboardPlayer player) {
      _avatar.sprite = null;
      _score.text = player.Score.ToString();
      Player = player;
   }
}
