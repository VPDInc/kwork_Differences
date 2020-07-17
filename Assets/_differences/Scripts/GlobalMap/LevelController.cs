using System.Collections.Generic;

using Airion.Currency;

using Lean.Touch;

using UnityEngine;

using Zenject;

public class LevelController : MonoBehaviour {
    public int LastLevelNum => _lastLevelNum;
    public int CompleteRatingReward => _completeRatingReward;
    public int CompleteCoinReward => _completeCoinReward;

    [SerializeField] int _completeCoinReward = 25;
    [SerializeField] int _completeRatingReward = 150;

    [Inject] LeanDragCamera _leanDragCamera = default;
    [Inject] UILevelStartView _levelStartView = default;
    [Inject] UIFinishLevelView _uiFinishLevelView = default;
    [Inject] GameplayController _gameplay = default;
    [Inject] CurrencyManager _currencyManager = default;
    [Inject] EnergyController _energyController = default;
    [Inject] Tournament _tournament = default;
    [Inject] Database _database = default;

    Currency _coinCurrency = default;
    Currency _ratingCurrency = default;
    
    int _lastLevelNum = 0;
    List<LevelInfo> _allLevels = new List<LevelInfo>();
    LevelInfo _currentLevel;
    
    const string LAST_LEVEL_ID = "last_level";
    const string COIN_CURRENCY_ID = "Soft";
    const string RATING_CURRENCY_ID = "Rating";

    void Start() {
        _coinCurrency = _currencyManager.GetCurrency(COIN_CURRENCY_ID);
        _ratingCurrency = _currencyManager.GetCurrency(RATING_CURRENCY_ID);
        LoadLastLevel();
        SetupLevels();
        _gameplay.Completed += OnCompleted;
        _gameplay.Initialized += OnGameplayInit;
        _leanDragCamera.MoveTo(_allLevels[Mathf.Clamp(_lastLevelNum, 0, _allLevels.Count-1)].transform.position, true);
        _allLevels[(int)Mathf.Clamp(_lastLevelNum-1, 0, Mathf.Infinity)].SetAvatar(true);

        _database.Load(_lastLevelNum);
        _database.Load(_lastLevelNum + 1);
    }

    void OnDestroy() {
        _gameplay.Completed -= OnCompleted;
        _gameplay.Initialized -= OnGameplayInit;
    }

    void CompleteLevel(int num) {
        if(_lastLevelNum > 0)
            _allLevels[_lastLevelNum-1].SetAvatar(false);
        if(num >= _lastLevelNum)
            _lastLevelNum = num + 1;
        var level = _allLevels[Mathf.Clamp(num, 0, _allLevels.Count-1)];
        level.CompleteLevel();
        _leanDragCamera.MoveTo(level.transform.position, false);
        SaveLastLevel();
        
        
        level.SetAvatar(true);

        if(num + 1 < _allLevels.Count)
            _allLevels[num+1].UnlockLevel(false);
    }
    
    void OnCompleted(GameplayResult gameplayResult) {
        var coinsToEarn = gameplayResult.IsCompleted ? _completeCoinReward : 0;
        var ratingToEarn = gameplayResult.TotalStarsCollected + _completeRatingReward;
        _coinCurrency.Earn(coinsToEarn);
        _ratingCurrency.Earn(ratingToEarn);
        _tournament.AddScore(ratingToEarn);
        _uiFinishLevelView.Show(_lastLevelNum, gameplayResult, coinsToEarn);

        if (gameplayResult.IsCompleted) {
            // Cause current level num + 1 already loaded and we just need to load level after that
            _database.Load(_lastLevelNum + 2);
            CompleteLevel(_lastLevelNum);
        } else {
            // Reload current level with others pictures
            _database.Load(_lastLevelNum);            
        }
    }

    public void AddLevelToList(IEnumerable<LevelInfo> levelInfos) {
        _allLevels.AddRange(levelInfos);
    }

    public void OpenLastPlayView() {
        if(_energyController.IsCanPlay)
            OpenPlayView(_lastLevelNum);
    }

    void OpenPlayView(int levelNum) {
        _gameplay.Load(levelNum);
    }

    void PlayLevel(int levelNum) {
        _energyController.SpendPlayCost();
        _currentLevel = _allLevels[Mathf.Clamp(levelNum, 0, _allLevels.Count - 1)];
        _gameplay.Begin();
    }

    void SetupLevels() {
        foreach (LevelInfo levelInfo in _allLevels) {
            var levelNum = levelInfo.LevelNum;
            var isCompleted = _lastLevelNum > 0 && levelNum < LastLevelNum;
            var isUnlocked = levelNum <= LastLevelNum;
            levelInfo.Setup(isUnlocked, isCompleted);
        }
    }

    void SaveLastLevel() {
        PlayerPrefs.SetInt(LAST_LEVEL_ID, _lastLevelNum);
    }

    void LoadLastLevel() {
        _lastLevelNum = PlayerPrefs.GetInt(LAST_LEVEL_ID, 0);
    }
    
    void OnGameplayInit(int levelNum, Data[] data) {
        _levelStartView.SetLevelName(levelNum);
        
        _levelStartView.SetPicturesCount(data.Length);
        _levelStartView.SetDifferencesCount(data[0].Points.Length);

        _levelStartView.StartTimer(()=>PlayLevel(levelNum));
        _levelStartView.Show();
    }
}