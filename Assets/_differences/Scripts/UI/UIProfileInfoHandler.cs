using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIProfileInfoHandler : MonoBehaviour {
    [SerializeField] Image _profileIcon = default;
    [SerializeField] TMP_Text _nameLabel = default;

    [Inject] PlayerInfoController _playerInfoController = default;

    void Start() {
        UpdateInfo();

        _playerInfoController.InfoUpdated += UpdateInfo;
    }

    void OnDestroy() {
        _playerInfoController.InfoUpdated -= UpdateInfo;
    }

    void UpdateInfo() {
        _profileIcon.sprite = _playerInfoController.PlayerIcon;
        _nameLabel.text = _playerInfoController.PlayerName;
    }
}