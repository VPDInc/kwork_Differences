using System;

using Facebook.Unity;

using PlayFab.ClientModels;

using UnityEngine;

using Zenject;

using Random = UnityEngine.Random;

public class PlayerInfoController : MonoBehaviour {
    public event Action InfoUpdated;

    [SerializeField] Sprite[] _profileIcons = default;

    [Inject] PlayFabFacebook _playFabFacebook = default;
    [Inject] PlayFabLogin _playFabLogin = default;

    int _playerIconId = default;
    string _playerName = default;
    Sprite _facebookIcon = default;
    bool _isFacebookIconAvailable = default;

    const string NAME_ID = "name_id";
    const string ICON_ID = "icon_id";

    void Awake() {
        LoadName();
        LoadPlayerIcon();
    }

    void Start() {
        _playFabFacebook.FacebookLogged += RequestFacebookAvatar;
        _playFabFacebook.FacebookLinked += RequestFacebookAvatar;
        _playFabFacebook.FacebookUnlinked += OnFacebookUnlinked;
        
        _playFabLogin.AccountInfoRecieved += OnAccountInfoRecieved;
    }

    void OnAccountInfoRecieved(GetAccountInfoResult obj) {
        if(_playFabLogin.IsFacebookLinked)
            RequestFacebookAvatar();
    }

    void OnDestroy() {
        _playFabFacebook.FacebookLogged -= RequestFacebookAvatar;
        _playFabFacebook.FacebookLinked -= RequestFacebookAvatar;
        _playFabFacebook.FacebookUnlinked -= OnFacebookUnlinked;
        
        _playFabLogin.AccountInfoRecieved -= OnAccountInfoRecieved;
    }

    void OnFacebookUnlinked() {
        _isFacebookIconAvailable = false;
        InfoUpdated?.Invoke();
    }

    void LoadName() {
        _playerName = PlayerPrefs.GetString(NAME_ID, "user" + Random.Range(10000, 100000));
        InfoUpdated?.Invoke();
    }

    void SaveName() {
        PlayerPrefs.SetString(NAME_ID, _playerName);
    }

    void LoadPlayerIcon() {
        _playerIconId = PlayerPrefs.GetInt(ICON_ID, 0);
        InfoUpdated?.Invoke();
    }

    void SavePlayerIcon() {
        PlayerPrefs.SetInt(ICON_ID, _playerIconId);
    }

    public string PlayerName {
        get => _playerName;
        set {
            _playerName = value;
            InfoUpdated?.Invoke();
            SaveName();
        }
    }

    public int IconId => _playerIconId;

    public Sprite SetIcon(int id) {
        _playerIconId = id;
        SavePlayerIcon();
        InfoUpdated?.Invoke();
        return _profileIcons[Mathf.Clamp(id, 0, _profileIcons.Length)];
    }

    public Sprite PlayerIcon => _isFacebookIconAvailable ? _facebookIcon : _profileIcons[Mathf.Clamp(_playerIconId, 0, _profileIcons.Length)];

    public Sprite[] ProfileIcons => _profileIcons;

    void RequestFacebookAvatar() {
        if(_isFacebookIconAvailable) return;
        Debug.Log("Avatar requested");
        FB.API("me/picture?type=square&height=200&width=200", HttpMethod.GET, GetPicture);
    }

    void GetPicture(IGraphResult result) {
        if (result.Error == null) {
            _facebookIcon = Sprite.Create(result.Texture, new Rect(0, 0, 200, 200), new Vector2());
            _isFacebookIconAvailable = true;
            InfoUpdated?.Invoke();
        } else {
            Debug.LogError(result.Error);
        }
    }
}