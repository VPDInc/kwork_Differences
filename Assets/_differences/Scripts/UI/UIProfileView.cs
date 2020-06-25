using System;
using System.Collections;
using System.Collections.Generic;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;

using Zenject;

public class UIProfileView : MonoBehaviour {
    [SerializeField] string _versionPrefix = "v ";
    [SerializeField] string _userIdPrefix = "uid: ";
    
    [SerializeField] TMP_Text _versionText = default;
    [SerializeField] TMP_Text _userIdText = default;
    
    [SerializeField] GameObject _linkFacebookButton = default;
    [SerializeField] GameObject _facebookLinkedPanel = default;
    
    [Inject] PlayFabLogin _playFabLogin = default;
    [Inject] PlayFabFacebook _playFabFacebook = default;
    
    UIView _currentView = default;

    void Awake() {
        _currentView = GetComponent<UIView>();
    }

    void Start() {
        UpdateInfo();
        
        _playFabFacebook.FacebookLinked += UpdateInfo;
        _playFabFacebook.FacebookUnlinked += UpdateInfo;
    }

    void OnDestroy() {
        _playFabFacebook.FacebookLinked -= UpdateInfo;
        _playFabFacebook.FacebookUnlinked -= UpdateInfo;
    }

    public void Show(bool instant) {
        _currentView.Show(instant);
    }

    public void Hide(bool instant) {
        _currentView.Hide(instant);
    }
    
    void UpdateInfo() {
        _versionText.text = _versionPrefix + Application.version;
        _userIdText.text = _userIdPrefix + _playFabLogin.AccountInfo.AccountInfo.PlayFabId;
        
        _linkFacebookButton.SetActive(!_playFabLogin.IsFacebookLinked);
        _facebookLinkedPanel.SetActive(_playFabLogin.IsFacebookLinked);
    }
}