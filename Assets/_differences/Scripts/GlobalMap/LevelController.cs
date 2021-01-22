using System;
using System.Collections.Generic;

using Airion.Currency;
using Airion.Extensions;
using Differences;
using Doozy.Engine;

using Lean.Touch;

using UnityEngine;

using Zenject;

public class LevelController : Singleton<LevelController> {
    public int LastLevelNum => _lastLevelNum;
    public int LastEpisodeNum => _lastEpisodeNum;
    public int CompleteRatingReward => _completeRatingReward;
    public int CompleteCoinReward => _completeCoinReward;

    [SerializeField] int _completeCoinReward = 25;
    [SerializeField] int _completeRatingReward = 150;

    [Inject] LeanDragCamera _leanDragCamera = default;
    [Inject] UILevelStartView _levelStartView = default;
    [Inject] UIFinishLevelView _uiFinishLevelView = default;
    [Inject] GameplayController _gameplay = default;
    [Inject] EnergyController _energyController = default;
    [Inject] Tournament _tournament = default;
    [Inject] Database _database = default;
    
    int _lastLevelNum = 0;
    int _lastEpisodeNum = 0;
    List<LevelInfo> _allLevels = new List<LevelInfo>();
    LevelInfo _currentLevel;
    float _startLevelTimestamp;

    int Try {
        get => PlayerPrefs.GetInt("try", 0);
        set => PlayerPrefs.SetInt("try", value);
    }
    
    const string LAST_LEVEL_ID = "last_level";
    const string LAST_EPISODE_ID = "last_episode";

    const string OPEN_STORE_EVENT_ID = "OpenEnergyStore";

    protected override void Awake() {
        base.Awake();
        _lastEpisodeNum = PlayerPrefs.GetInt(LAST_EPISODE_ID, 0);
        LoadLastLevel();
    }

    void Start() {
        SetupLevels();
        _gameplay.Completed += OnCompleted;
        _gameplay.Initialized += OnGameplayInit;
        _leanDragCamera.MoveTo(_allLevels[Mathf.Clamp(_lastLevelNum, 0, _allLevels.Count-1)].transform.position, true);
        _allLevels[Mathf.Max(_lastLevelNum, 0)].SetAvatar(true);

        _database.Load(_lastLevelNum);
    }

    void OnDestroy() {
        _gameplay.Completed -= OnCompleted;
        _gameplay.Initialized -= OnGameplayInit;
    }
    
    public static int GetLastLevelNum() {
        if (Instance == null)
            return 0;

        return Instance.LastLevelNum;
    }

    void CompleteLevel(int num)
    {
        _allLevels[_lastLevelNum].SetAvatar(false);
        if(num >= _lastLevelNum)
            _lastLevelNum = num + 1;
        var level = _allLevels[Mathf.Clamp(num, 0, _allLevels.Count-1)];
        level.CompleteLevel();
        _leanDragCamera.MoveTo(level.transform.position, false);
        SaveLastLevel();

        if (_lastEpisodeNum < level.EpisodeInfo.EpisodeNum)
        {
            _lastEpisodeNum = level.EpisodeInfo.EpisodeNum;
            PlayerPrefs.SetInt(LAST_EPISODE_ID, _lastEpisodeNum);
        }
        
        _allLevels[_lastLevelNum].SetAvatar(true);

        if(num + 1 < _allLevels.Count)
            _allLevels[num+1].UnlockLevel(false);

        _database.Load(_lastLevelNum);
    }
    
    void OnCompleted(GameplayResult gameplayResult) {
        var coinsToEarn = gameplayResult.IsCompleted ? _completeCoinReward : 0;
        Analytic.CurrencyEarn(coinsToEarn, "level-completed", LastLevelNum.ToString());
        _uiFinishLevelView.Show(_lastLevelNum, gameplayResult, coinsToEarn);
        if (gameplayResult.IsCompleted) {
            var ratingToEarn = gameplayResult.TotalStarsCollected + _completeRatingReward;
            _tournament.AddScore(ratingToEarn);
            CompleteLevel(_lastLevelNum);
            Analytic.LogComplete(_lastLevelNum, Time.time - _startLevelTimestamp, Try);
            Try = 0;
        } else {
            Analytic.LogFail(_lastLevelNum);
            // Reload current level with others pictures
            _database.Load(_lastLevelNum);            
        }
    }

    public void AddLevelToList(IEnumerable<LevelInfo> levelInfos) {
        _allLevels.AddRange(levelInfos);
    }

    public void OpenLastPlayView() {
        if (_energyController.IsCanPlay) {
            OpenPlayView(_lastLevelNum);
        } else {
            GameEventMessage.SendEvent(OPEN_STORE_EVENT_ID);
        }
    }

    void OpenPlayView(int levelNum) {
        _gameplay.Load(levelNum);
    }

    void PlayLevel(int levelNum) {
        _energyController.SpendPlayCost();
        _currentLevel = _allLevels[Mathf.Clamp(levelNum, 0, _allLevels.Count - 1)];
        _gameplay.Begin();
        Analytic.LogStartLevel(levelNum);
        Try++;
        _startLevelTimestamp = Time.time;
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

    [ContextMenu("Complete")]
    void DebugComplete() {
        CompleteLevel(_lastLevelNum);
    }
}