using System;

using UnityEngine;

using Random = UnityEngine.Random;

public class PlayerInfoController : MonoBehaviour {
    public event Action InfoUpdated;
    
    [SerializeField] Sprite[] _profileIcons = default;
    
    int _playerIconId = default;
    string _playerName = default;

    const string NAME_ID = "name_id";
    const string ICON_ID = "icon_id";

    void Awake() {
        LoadName();
        LoadPlayerIcon();
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

    public Sprite SetIcon(int id) {
        _playerIconId = id;
        SavePlayerIcon();
        InfoUpdated?.Invoke();
        return _profileIcons[Mathf.Clamp(id, 0, _profileIcons.Length)];
    }
    
    public Sprite PlayerIcon => _profileIcons[Mathf.Clamp(_playerIconId, 0, _profileIcons.Length)];

    public Sprite[] ProfileIcons => _profileIcons;
}