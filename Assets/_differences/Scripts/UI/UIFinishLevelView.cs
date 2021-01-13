using System;
using System.Collections;
using System.Linq;
using Airion.Currency;
using Airion.Extensions;

using DG.Tweening;
using Differences;
using Doozy.Engine.UI;

using EasyMobile;

using TMPro;

using UnityEngine;

using Zenject;

public class UIFinishLevelView : MonoBehaviour
{
    private const string LEVEL_NAME_PREFIX = "Level ";
    private const string ROUND_NAME_PREFIX = "Round ";
    private const string LEVEL_COMPLETED_PREFIX = "Level completed:";

    [Header("UIVisualReferences")]
    [SerializeField] private GameObject[] _victoryObjects = default;
    [SerializeField] private GameObject[] _loseObjects = default;

    [Header("References")]
    [SerializeField] private TMP_Text _levelLabel = default;
    [SerializeField] private TMP_Text _coinRewardLabel = default;
    [SerializeField] private TMP_Text _energyRewardLabel = default;
    [SerializeField] private TMP_Text _ratingRewardLabel = default;
    [SerializeField] private Transform _textInfoHolder = default;
    [SerializeField] private PicturePanel _picturePanel = default;

    [SerializeField] private ItemEffect[] _winEffects;
    [SerializeField] private AudioSource _winAudioEffect;

    [Header("Prefabs")]
    [SerializeField] private RoundInfo _roundInfoPrefab = default;

    [Header("Rect References")]
    [SerializeField] private RectTransform _medalStartTransform = default;
    [SerializeField] private RectTransform _energyStartTransform = default;
    [SerializeField] private RectTransform _coinsStartTransform = default;
    [SerializeField] private RectTransform _medalEndTransform = default;
    [SerializeField] private RectTransform _energyEndTransform = default;
    [SerializeField] private RectTransform _coinsEndTransform = default;
    [SerializeField] private UITrailEffect _medalFlyingPrefab = default;
    [SerializeField] private UITrailEffect _energyFlyingPrefab = default;
    [SerializeField] private UITrailEffect _coinsFlyingPrefab = default;
    [SerializeField] private float _pauseBetweenSpawns = 1f;
    [SerializeField] private float _fxAmount = 5;

    [Inject] LevelController _levelController = default;
    [Inject] EnergyController _energyController = default;
    [Inject] CurrencyManager _currencyManager = default;


    Currency _coinCurrency = default;
    Currency _ratingCurrency = default;

    bool isErnReward = false;


    UIView _currentView = default;

    void Awake()
    {
        _coinCurrency = _currencyManager.GetCurrency(CurrencyConstants.SOFT);
        _ratingCurrency = _currencyManager.GetCurrency(CurrencyConstants.RATING);

        _currentView = GetComponent<UIView>();
    }

    public void Show(int levelNum, GameplayResult gameplayResult, int coinReward)
    {
        isErnReward = false;

        SetupVictory(gameplayResult.IsCompleted);
        Show();
        SetLevelName(levelNum);
        SetCoinsAmount(coinReward);
        Setup(gameplayResult);

        if (gameplayResult.IsCompleted)
        {
            SetupFlyingCurrencies(gameplayResult.TotalStarsCollected, _energyController.PlayCost, coinReward);
            StartCoroutine(TryRequestRateUs());
            StartCoroutine(PlayEffects(_winEffects, 2, 0.25f));
            StartCoroutine(PlayAudio());
        }
    }

    private IEnumerator PlayAudio()
    {
        yield return new WaitForSeconds(2.25f);
        _winAudioEffect.Play();
    }

    private IEnumerator PlayEffects(ItemEffect[] effects, float startTime, float delay)
    {
        yield return new WaitForSeconds(startTime);

        for (int i = 0; i < effects.Length; i++)
        {
            yield return new WaitForSeconds(delay * i);
            effects[i].Play();
        }
    }

    void SetupVictory(bool isVictory)
    {
        foreach (GameObject victoryObject in _victoryObjects)
            victoryObject.SetActive(isVictory);

        foreach (GameObject loseObject in _loseObjects)
            loseObject.SetActive(!isVictory);
    }

    IEnumerator TryRequestRateUs()
    {
        yield return new WaitForSeconds(2);
        if (!StoreReview.IsRatingRequestDisabled())
        {
            if (_levelController.LastLevelNum >= 3)
            {
                if (StoreReview.CanRequestRating())
                    StoreReview.RequestRating();
            }
        }
    }

    void SetupFlyingCurrencies(int medalAmount, int energyAmount, int coinsAmount)
    {
        var seq = DOTween.Sequence();
        seq.AppendInterval(3);
        seq.AppendCallback(() => {
                               // var pauseBetweenSpawn = _pauseBetweenSpawns / medalAmount;
            SetupTrailEffect(_medalFlyingPrefab, _medalStartTransform, _medalEndTransform);

        });

        // seq.AppendInterval(_pauseBetweenSpawns);
        seq.AppendCallback(() => {
            // var pauseBetweenSpawn = _pauseBetweenSpawns / energyAmount;

            SetupTrailEffect(_energyFlyingPrefab, _energyStartTransform, _energyEndTransform);

        });

        // seq.AppendInterval(_pauseBetweenSpawns);
        seq.AppendCallback(() => {
            // var pauseBetweenSpawn = _pauseBetweenSpawns / coinsAmount;

            SetupTrailEffect(_coinsFlyingPrefab, _coinsStartTransform, _coinsEndTransform, delegate { 
                //TODO 13.01.2021 REFACTORING
                if(!isErnReward)
                {
                    _coinCurrency.Earn(coinsAmount);
                    isErnReward = true;
                }
            });

        });
    }

    //TODO 13.01.2021 REFACTORING!
    float countfx = 0;
    void SetupTrailEffect(UITrailEffect uITrailEffect,Transform startTransform, RectTransform targetTransform, Action onSuccess = null)
    {
        for (int i = 0; i < _fxAmount; i++)
        {
            var coinFx = Instantiate(uITrailEffect, startTransform);
            coinFx.Setup(targetTransform.position, _pauseBetweenSpawns * i, delegate {

                ++countfx;

                if (_fxAmount == countfx)
                {
                    countfx = 0;
                    onSuccess?.Invoke();
                }
            });
        }
    }


    void Show() =>
        _currentView.Show();

    public void Hide() =>
        _currentView.Hide();

    void SetLevelName(int levelNum) =>
        _levelLabel.text = LEVEL_NAME_PREFIX + (levelNum + 1);

    void SetCoinsAmount(int coinsAmount) =>
        _coinRewardLabel.text = coinsAmount.ToString();

    void Setup(GameplayResult gameplayResult)
    {
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

    [System.Serializable]
    private class ItemEffect
    {
        [SerializeField] private GameObject _effect;
        [SerializeField] private Transform _position;

        public void Play()
        {
            Instantiate(_effect, _position, false);
        }
    }
}