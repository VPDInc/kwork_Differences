using System;
using System.Collections;
using System.Collections.Generic;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIPlayerInfoChange : MonoBehaviour {
    [SerializeField] ProfileIconElement _iconElementPrefab = default;
    [SerializeField] Transform _iconElementsHolder = default;
    [SerializeField] TMP_InputField _inputField = default;
    
    [Inject] PlayerInfoController _playerInfoController = default;

    UIView _currentView = default;

    void Awake() {
        _currentView = GetComponent<UIView>();
    }

    void Start() {
        FillImages();
    }

    public void Show() {
        _currentView.Show();
    }

    public void Hide() {
        _currentView.Hide();
    }

    void FillImages() {
        for (var i = 0; i < _playerInfoController.ProfileIcons.Length; i++) {
            Sprite profileIcon = _playerInfoController.ProfileIcons[i];
            var element = Instantiate(_iconElementPrefab, _iconElementsHolder);
            element.SetImage(profileIcon, i);
        }
    }

    public void Save() {
        _playerInfoController.PlayerName = _inputField.text;
        Hide();
    }
}