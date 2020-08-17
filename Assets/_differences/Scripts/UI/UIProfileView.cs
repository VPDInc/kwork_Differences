using Doozy.Engine.UI;

using TMPro;

using UnityEngine;

using Zenject;

public class UIProfileView : MonoBehaviour {
    [SerializeField] string _versionPrefix = "v ";
    [SerializeField] string _userIdPrefix = "uid: ";
    
    [SerializeField] TMP_Text _versionText = default;
    [SerializeField] TMP_Text _userIdText = default;
    [SerializeField] TMP_Text _levelText = default;
    
    [SerializeField] GameObject _linkFacebookButton = default;
    [SerializeField] GameObject _facebookLinkedPanel = default;
    [SerializeField] GameObject _editButton = default;
    
    [Inject] PlayFabInfo _playFabInfo = default;
    [Inject] PlayFabFacebook _playFabFacebook = default;
    [Inject] LevelController _levelController = default;
    
    UIView _currentView = default;

    void Awake() {
        _currentView = GetComponent<UIView>();
    }

    void Start() {
        if (_playFabInfo.AccountInfo != null)
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
        _levelText.text = "Level " + _levelController.LastLevelNum;
    }

    public void Hide(bool instant) {
        _currentView.Hide(instant);
    }
    
    void UpdateInfo() {
        _versionText.text = _versionPrefix + Application.version;
        _userIdText.text = _userIdPrefix + _playFabInfo.AccountInfo.AccountInfo.PlayFabId;
        
        _linkFacebookButton.SetActive(!_playFabInfo.IsFacebookLinked);
        _facebookLinkedPanel.SetActive(_playFabInfo.IsFacebookLinked);
        _editButton.SetActive(!_playFabInfo.IsFacebookLinked);
    }
}