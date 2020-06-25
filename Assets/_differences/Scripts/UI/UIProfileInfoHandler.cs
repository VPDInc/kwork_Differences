using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIProfileInfoHandler : MonoBehaviour {
    [SerializeField] Image _profileIcon = default;
    [SerializeField] TMP_Text _nameLabel = default;

    [Inject] PlayerInfoController _playerInfoController = default;
    [Inject] PlayFabFacebook _playFabFacebook = default;

    void Start() {
        UpdateInfo();

        _playerInfoController.InfoUpdated += UpdateInfo;
        _playFabFacebook.FacebookLinked += UpdateInfo;
        _playFabFacebook.FacebookUnlinked += UpdateInfo;
    }

    void OnEnable() {
        UpdateInfo();
    }

    void OnDestroy() {
        _playerInfoController.InfoUpdated -= UpdateInfo;
        _playFabFacebook.FacebookLinked -= UpdateInfo;
        _playFabFacebook.FacebookUnlinked -= UpdateInfo;
    }

    void UpdateInfo() {
        _profileIcon.sprite = _playerInfoController.PlayerIcon;
        _nameLabel.text = _playerInfoController.PlayerName;
    }
}