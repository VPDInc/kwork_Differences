using System.Collections;
using System.Linq;

using Airion.Extensions;

using DG.Tweening;

using Doozy.Engine.UI;

using EasyMobile;

using TMPro;

using UnityEngine;

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
    [SerializeField] PicturePanel _picturePanel = default;
    [SerializeField] ParticleSystem _particleSystem = default;

    [Header("Prefabs")] [SerializeField] RoundInfo _roundInfoPrefab = default;

    [Header("Rect References")] [SerializeField]
    RectTransform _medalStartTransform = default;
    [SerializeField] RectTransform _energyStartTransform = default;
    [SerializeField] RectTransform _coinsStartTransform = default;
    [SerializeField] RectTransform _medalEndTransform = default;
    [SerializeField] RectTransform _energyEndTransform = default;
    [SerializeField] RectTransform _coinsEndTransform = default;
    [SerializeField] UITrailEffect _medalFlyingPrefab = default;
    [SerializeField] UITrailEffect _energyFlyingPrefab = default;
    [SerializeField] UITrailEffect _coinsFlyingPrefab = default;
    [SerializeField] float _pauseBetweenSpawns = 1f;
    [SerializeField] float _fxAmount = 5;

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

        if (gameplayResult.IsCompleted) {
            SetupFlyingCurrencies(gameplayResult.TotalStarsCollected, _energyController.PlayCost, coinReward);
            StartCoroutine(TryRequestReateUs());
            _particleSystem.Play();
        }
    }

    void SetupVictory(bool isVictory) {
        foreach (GameObject victoryObject in _victoryObjects) {
            victoryObject.SetActive(isVictory);
        }

        foreach (GameObject loseObject in _loseObjects) {
            loseObject.SetActive(!isVictory);
        }
    }

    IEnumerator TryRequestReateUs() {
        yield return new WaitForSeconds(2);
        if (!StoreReview.IsRatingRequestDisabled()) {
            if (_levelController.LastEpisodeNum >= 3) {
                if (StoreReview.CanRequestRating())
                    StoreReview.RequestRating();
            }
        }
    }

    void SetupFlyingCurrencies(int medalAmount, int energyAmount, int coinsAmount) {
        var seq = DOTween.Sequence();
        seq.AppendInterval(3);
        seq.AppendCallback(() => {
                               // var pauseBetweenSpawn = _pauseBetweenSpawns / medalAmount;
                               for (int i = 0; i < _fxAmount; i++) {
                                   var medalFx = Instantiate(_medalFlyingPrefab, _medalStartTransform);
                                   medalFx.Setup(_medalEndTransform.position, _pauseBetweenSpawns * i);
                               }
                           });

        // seq.AppendInterval(_pauseBetweenSpawns);
        seq.AppendCallback(() => {
                               // var pauseBetweenSpawn = _pauseBetweenSpawns / energyAmount;
                               for (int i = 0; i < _fxAmount; i++) {
                                   var energyFx = Instantiate(_energyFlyingPrefab, _energyStartTransform);
                                   energyFx.Setup(_energyEndTransform.position, _pauseBetweenSpawns * i);
                               }
                           });

        // seq.AppendInterval(_pauseBetweenSpawns);
        seq.AppendCallback(() => {
                               // var pauseBetweenSpawn = _pauseBetweenSpawns / coinsAmount;
                               for (int i = 0; i < _fxAmount; i++) {
                                   var coinFx = Instantiate(_coinsFlyingPrefab, _coinsStartTransform);
                                   coinFx.Setup(_coinsEndTransform.position, _pauseBetweenSpawns * i);
                               }
                           });
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
        var ratingToEarn = gameplayResult.TotalStarsCollected +
                           (gameplayResult.IsCompleted ? _levelController.CompleteRatingReward : 0);

        var energyToEarn = gameplayResult.IsCompleted ? _energyController.PlayCost : 0;

        _coinRewardLabel.text = coinsToEarn.ToString();
        _ratingRewardLabel.text = ratingToEarn.ToString();
        _energyRewardLabel.text = energyToEarn.ToString();

        _picturePanel.FillByImages(gameplayResult.PictureResults);
    }
}