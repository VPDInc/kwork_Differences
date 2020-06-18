﻿using System.Linq;

using Airion.Extensions;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class UIFinishLevelView : MonoBehaviour {
    [Header("UIVisualReferences")] [SerializeField]
    GameObject[] _victoryObjects = default;
    [SerializeField] GameObject[] _loseObjects = default;

    [Header("References")] [SerializeField]
    TMP_Text _levelLabel = default;
    [SerializeField] TMP_Text _coinRewardLabel = default;
    [SerializeField] TMP_Text _playerNameLabel = default;
    [SerializeField] Image _profileIcon = default;
    [SerializeField] Transform _infoHolder = default;

    [Header("Prefabs")] [SerializeField] UIPictureResultInfo _pictureResultInfoPrefab = default;

    UIView _currentView = default;

    const string LEVEL_NAME_PREFIX = "Level ";

    void Awake() {
        _currentView = GetComponent<UIView>();
    }
    
    public void Show(int levelNum, GameplayResult gameplayResult, int coinReward) {
        SetupVictory(gameplayResult.IsCompleted);
        Show();
        SetLevelName(levelNum);
        SetCoinsAmount(coinReward);
        Setup(gameplayResult);
    }

    void SetupVictory(bool isVictory) {
        foreach (GameObject victoryObject in _victoryObjects) {
            victoryObject.SetActive(isVictory);
        }

        foreach (GameObject loseObject in _loseObjects) {
            loseObject.SetActive(!isVictory);
        }
    }

    void Show() {
        _currentView.Show();
    }

    public void Hide() {
        _currentView.Hide();
    }

    void SetLevelName(int levelNum) {
        _levelLabel.text = LEVEL_NAME_PREFIX + (levelNum + 1);
    }

    void SetCoinsAmount(int coinsAmount) {
        _coinRewardLabel.text = coinsAmount.ToString();
    }

    void SetPlayerName(string name) {
        _playerNameLabel.text = name;
    }

    public void SetPlayerProfileIcon(Sprite icon) {
        _profileIcon.sprite = icon;
    }

    //DUMMY
    //TODO: Decide how to get game results
    void Setup(GameplayResult gameplayResult) {
        _infoHolder.DestroyAllChildren();

        for (int i = 0; i < gameplayResult.PicturesCount; i++) {
            var info = Instantiate(_pictureResultInfoPrefab, _infoHolder);
            var differencePoints = gameplayResult.PictureResults[i].DifferencePoints;
            //TODO: Implement ranking score
            info.Setup(differencePoints, gameplayResult.PictureResults[i].StarsCollected);
        }
    }
}