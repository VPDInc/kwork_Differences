using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIProfileInfoHandler : MonoBehaviour {
    [SerializeField] string _versionPrefix = "v ";
    [SerializeField] string _userIdPrefix = "uid: ";
    [SerializeField] Image _profileIcon = default;
    [SerializeField] TMP_Text _nameLabel = default;
    [SerializeField] TMP_Text _versionText = default;
    [SerializeField] TMP_Text _userIdText = default;
    [SerializeField] GameObject _linkFacebookButton = default;
    [SerializeField] GameObject _facebookLinkedPanel = default;

    [Inject] PlayerInfoController _playerInfoController = default;
    [Inject] PlayFabLogin _playFabLogin = default;
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
        _versionText.text = _versionPrefix + Application.version;
        _userIdText.text = _userIdPrefix + _playFabLogin.AccountInfo.AccountInfo.PlayFabId;
        
        _linkFacebookButton.SetActive(!_playFabLogin.IsFacebookLinked);
        _facebookLinkedPanel.SetActive(_playFabLogin.IsFacebookLinked);
    }
}