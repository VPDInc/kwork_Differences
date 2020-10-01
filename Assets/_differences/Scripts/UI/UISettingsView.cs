using System.Collections;
using System.Collections.Generic;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;
using UnityEngine.Analytics;

public class UISettingsView : MonoBehaviour {
    [SerializeField] string _versionPrefix = "v ";
    [SerializeField] string _userIdPrefix = "uid: ";
    [SerializeField] TMP_Text _tmpText = default;
    [SerializeField] TMP_Text _userIdText = default;
    
    UIView _currentView = default;

    void Awake() {
        _currentView = GetComponent<UIView>();

        _tmpText.text = _versionPrefix + Application.version;
        _userIdText.text = _userIdPrefix + AnalyticsSessionInfo.userId;
    }

    public void Show(bool instant) {
        _currentView.Show(instant);
    }

    public void Hide(bool instant) {
        _currentView.Hide(instant);
    }
}