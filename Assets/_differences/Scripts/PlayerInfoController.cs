using System;
using System.Collections;

using Facebook.Unity;

using PlayFab.ClientModels;

using UnityEngine;

using Zenject;

using Random = UnityEngine.Random;

public class PlayerInfoController : MonoBehaviour {
    public event Action InfoUpdated;

    [SerializeField] Sprite[] _profileIcons = default;

    [Inject] PlayFabFacebook _playFabFacebook = default;
    [Inject] PlayFabInfo _playFabInfo = default;

    int _playerIconId = default;
    string _playerName = default;
    string _facebookName = default;
    Sprite _facebookIcon = default;
    bool _isFacebookIconAvailable = default;
    bool _isFacebookNameAvailable = default;
    bool _isFirstLaunch = default;

    const string NAME_ID = "name_id";
    const string ICON_ID = "icon_id";

    void Awake() {
        LoadName();
        LoadPlayerIcon();
    }

    public Sprite GetRandomIcon() {
        return _profileIcons[Random.Range(0, _profileIcons.Length)];
    }

    void Start() {
        _playFabFacebook.FacebookLogged += RequestFacebookInfo;
        _playFabFacebook.FacebookLinked += RequestFacebookInfo;
        _playFabFacebook.FacebookUnlinked += OnFacebookUnlinked;

        _playFabInfo.AccountInfoRecieved += OnAccountInfoRecieved;
    }

    void OnAccountInfoRecieved(GetAccountInfoResult obj) {
        if (_playFabFacebook.IsFacebookLogged) {
            RequestFacebookAvatar();
            RequestFacebookName();
        }

        var username = _playFabInfo.GetName();
        if (!string.IsNullOrEmpty(username)) {
            _playerName = username;
        } else if (_isFirstLaunch) {
            SaveName();
            _isFirstLaunch = false;
        }
    }

    void OnDestroy() {
        _playFabFacebook.FacebookLogged -= RequestFacebookInfo;
        _playFabFacebook.FacebookLinked -= RequestFacebookInfo;
        _playFabFacebook.FacebookUnlinked -= OnFacebookUnlinked;

        _playFabInfo.AccountInfoRecieved -= OnAccountInfoRecieved;
    }

    void OnFacebookUnlinked() {
        _isFacebookIconAvailable = false;
        _isFacebookNameAvailable = false;
        InfoUpdated?.Invoke();
    }

    void RequestFacebookInfo() {
        RequestFacebookAvatar();
        RequestFacebookName();
    }

    void LoadName() {
        _isFirstLaunch = !PlayerPrefs.HasKey(NAME_ID);
        _playerName = PlayerPrefs.GetString(NAME_ID, "user" + Random.Range(10000, 100000));

        InfoUpdated?.Invoke();
    }

    void SaveName() {
        PlayerPrefs.SetString(NAME_ID, PlayerName);
        _playFabInfo.SetPlayFabName(PlayerName);
    }

    void LoadPlayerIcon() {
        _playerIconId = PlayerPrefs.GetInt(ICON_ID, 0);
        InfoUpdated?.Invoke();
    }

    void SavePlayerIcon() {
        PlayerPrefs.SetInt(ICON_ID, _playerIconId);
    }

    public string PlayerName {
        get => _isFacebookNameAvailable ? _facebookName : _playerName;
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

    public Sprite PlayerIcon => _isFacebookIconAvailable
                                    ? _facebookIcon
                                    : _profileIcons[Mathf.Clamp(_playerIconId, 0, _profileIcons.Length)];

    public Sprite[] ProfileIcons => _profileIcons;

    void RequestFacebookAvatar() {
        if (_isFacebookIconAvailable) return;
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

    void RequestFacebookName() {
        if (_isFacebookNameAvailable) return;
        Debug.Log("Name requested");
        FB.API("me?fields=first_name", HttpMethod.GET, GetFacebookName);
    }

    void GetFacebookName(IGraphResult result) {
        if (result.Error == null) {
            IDictionary dict = Facebook.MiniJSON.Json.Deserialize(result.RawResult) as IDictionary;
            _facebookName = dict["first_name"].ToString();
            _isFacebookNameAvailable = true;
            InfoUpdated?.Invoke();
            SaveName();
        } else {
            Debug.LogError(result.Error);
        }
    }
}