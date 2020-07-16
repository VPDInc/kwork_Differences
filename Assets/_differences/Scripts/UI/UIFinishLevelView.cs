using System.Linq;

using Airion.Extensions;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIFinishLevelView : MonoBehaviour {
    [Header("UIVisualReferences")] [SerializeField]
    GameObject[] _victoryObjects = default;
    [SerializeField] GameObject[] _loseObjects = default;

    [Header("References")] [SerializeField]
    TMP_Text _levelLabel = default;
    [SerializeField] TMP_Text _coinRewardLabel = default;
    [SerializeField] TMP_Text _energyRewardLabel = default;
    [SerializeField] TMP_Text _ratingRewardLabel = default;
    [SerializeField] Transform _textInfoHolder = default;

    [Header("Prefabs")] [SerializeField] RoundInfo _roundInfoPrefab = default;

    [Inject] LevelController _levelController = default;
    [Inject] EnergyController _energyController = default;

    UIView _currentView = default;

    const string LEVEL_NAME_PREFIX = "Level ";
    const string ROUND_NAME_PREFIX = "Round ";
    const string LEVEL_COMPLETED_PREFIX = "Level completed:";

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
    
    void Setup(GameplayResult gameplayResult) {
        _textInfoHolder.DestroyAllChildren();

        for (int i = 0; i < gameplayResult.PicturesCount; i++) {
            var info = Instantiate(_roundInfoPrefab, _textInfoHolder);
            info.Setup(ROUND_NAME_PREFIX + (i + 1) + ":", gameplayResult.PictureResults[i].StarsCollected.ToString());
        }

        if (gameplayResult.IsCompleted) {
            var completeInfo = Instantiate(_roundInfoPrefab, _textInfoHolder);
            completeInfo.Setup(LEVEL_COMPLETED_PREFIX, _levelController.CompleteRatingReward.ToString());
        }
        
        var coinsToEarn = gameplayResult.IsCompleted ? _levelController.CompleteCoinReward : 0;
        var ratingToEarn = gameplayResult.TotalStarsCollected + (gameplayResult.IsCompleted ?_levelController.CompleteRatingReward : 0);
        var energyToEarn = gameplayResult.IsCompleted ? _energyController.PlayCost : 0;

        _coinRewardLabel.text = coinsToEarn.ToString();
        _ratingRewardLabel.text = ratingToEarn.ToString();
        _energyRewardLabel.text = energyToEarn.ToString();
    }
}