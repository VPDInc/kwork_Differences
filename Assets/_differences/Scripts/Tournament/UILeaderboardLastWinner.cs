using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UILeaderboardLastWinner : MonoBehaviour {
   public LeaderboardPlayer Player { get; private set; }
   public Image PlayerIcon => _avatar;
   
   [SerializeField] Image _avatar = default;
   [SerializeField] TextMeshProUGUI _displayName = default;
   [SerializeField] TextMeshProUGUI _score = default;
   [SerializeField] TextMeshProUGUI _reward = default;

   int _index;

   [Inject] TournamentRewards _tournamentRewards = default;
   
   public void Fill(int index, LeaderboardPlayer player) {
      _avatar.sprite = null;
      _displayName.text = player.DisplayName;
      _score.text = player.Score.ToString();
      _index = index;
      Player = player;
      _reward.text = _tournamentRewards.GetRewardByPlace(_index).ToString();
   }
}
