using Facebook.Unity;

using PlayFab;
using PlayFab.ClientModels;

using Sirenix.Utilities;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class LeaderboardElement : MonoBehaviour {
    [SerializeField] TextMeshProUGUI _positionText = default;
    [SerializeField] Image _avatar = default;
    [SerializeField] TextMeshProUGUI _displayName = default;
    [SerializeField] TextMeshProUGUI _score = default;
    [SerializeField] TextMeshProUGUI _reward = default;
    [SerializeField] Image _back = default;
    [SerializeField] Color _backColorIfMe = Color.magenta;

    [Inject] PlayerInfoController _infoController = default;

    public void Fill(int index, LeaderboardPlayer player) {
        _positionText.text = index.ToString();
        _avatar.sprite = null;
        _displayName.text = player.DisplayName.IsNullOrWhitespace() ? player.Id : player.DisplayName;
        _score.text = player.Score.ToString();
        _reward.text = "0";
        if (player.IsMe) {
            _avatar.sprite = _infoController.PlayerIcon;
            _back.color = _backColorIfMe;
            return;
        }

        var isLoaded = false;
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest() {
            PlayFabId = player.Id
        }, result => {
            if (result.AccountInfo.FacebookInfo != null) {
                FB.API($"{result.AccountInfo.FacebookInfo.FacebookId}/picture?type=square&height=200&width=200", HttpMethod.GET,
                    res => {
                        _avatar.sprite = Sprite.Create(res.Texture, new Rect(0, 0, 200, 200), new Vector2());
                        isLoaded = true;
                    });
            }
            
        }, err => {Debug.Log(err.GenerateErrorReport());});

        if (!isLoaded)
            _avatar.sprite = _infoController.GetRandomIcon();
    }
}
