using System.Collections.Generic;
using Airion.Extensions;
using Doozy.Engine;
using Lean.Touch;
using UnityEngine;
using Zenject;

public class LevelController : Singleton<LevelController>
{
    private const string LAST_LEVEL_ID = "last_level";
    private const string LAST_EPISODE_ID = "last_episode";
    private const string OPEN_STORE_EVENT_ID = "OpenEnergyStore";

    [SerializeField] private int _completeCoinReward = 25;
    [SerializeField] private int _completeRatingReward = 150;

    [Inject] private LeanDragCamera _leanDragCamera = default;
    [Inject] private UILevelStartView _levelStartView = default;
    [Inject] private UIFinishLevelView _uiFinishLevelView = default;
    [Inject] private GameplayController _gameplay = default;
    [Inject] private EnergyController _energyController = default;
    [Inject] private Database _database = default;

    private int _lastLevelNumber = 0;
    private int _currentLevelNumber = 0;

    private int _lastEpisodeNum = 0;
    private List<LevelInfo> _allLevels = new List<LevelInfo>();
    private float _startLevelTimestamp;

    private int Try
    {
        get => PlayerPrefs.GetInt("try", 0);
        set => PlayerPrefs.SetInt("try", value);
    }

    public int LastLevelNum => _lastLevelNumber;
    public int LastEpisodeNum => _lastEpisodeNum;
    public int CompleteRatingReward => _completeRatingReward;
    public int CompleteCoinReward => _completeCoinReward;

    protected override void Awake()
    {
        base.Awake();
        _lastEpisodeNum = PlayerPrefs.GetInt(LAST_EPISODE_ID, 0);
        LoadLastLevel();
    }

    private void Start()
    {
        SetupLevels();
        _gameplay.Completed += OnCompleted;
        _gameplay.Initialized += OnGameplayInit;
        _leanDragCamera.MoveTo(_allLevels[Mathf.Clamp(_lastLevelNumber, 0, _allLevels.Count - 1)].transform.position, true);
        _allLevels[Mathf.Max(_lastLevelNumber, 0)].SetAvatar(true);
    }

    private void OnDestroy()
    {
        _gameplay.Completed -= OnCompleted;
        _gameplay.Initialized -= OnGameplayInit;
    }

    public static int GetLastLevelNum()
    {
        if (Instance == null) return 0;
        return Instance.LastLevelNum;
    }

    private void CompleteLevel(int number)
    {
        if (_currentLevelNumber == _lastLevelNumber)
        {
            _allLevels[_lastLevelNumber].SetAvatar(false);
            if (number >= _lastLevelNumber) _lastLevelNumber = number + 1;
            var level = _allLevels[Mathf.Clamp(number, 0, _allLevels.Count - 1)];
            level.CompleteLevel();
            _leanDragCamera.MoveTo(level.transform.position, false);
            SaveLastLevel();

            if (_lastEpisodeNum < level.EpisodeInfo.EpisodeNum)
            {
                _lastEpisodeNum = level.EpisodeInfo.EpisodeNum;
                PlayerPrefs.SetInt(LAST_EPISODE_ID, _lastEpisodeNum);
            }

            _allLevels[_lastLevelNumber].SetAvatar(true);

            if (number + 1 < _allLevels.Count)
                _allLevels[number + 1].UnlockLevel(false);
        }
    }

    private void OnCompleted(GameplayResult gameplayResult)
    {
        var coinsToEarn = gameplayResult.IsCompleted ? _completeCoinReward : 0;
        var ratingToEarn = gameplayResult.TotalStarsCollected + _completeRatingReward;

        Analytic.CurrencyEarn(coinsToEarn, "level-completed", LastLevelNum.ToString());

        _uiFinishLevelView.Show(_currentLevelNumber, gameplayResult, coinsToEarn, ratingToEarn);

        if (gameplayResult.IsCompleted)
        {
            CompleteLevel(_currentLevelNumber);

            Analytic.LogComplete(_currentLevelNumber, Time.time - _startLevelTimestamp, Try);
            Try = 0;
        }
        else Analytic.LogFail(_currentLevelNumber);
    }

    public void AddLevelToList(IEnumerable<LevelInfo> levelInfos) =>
        _allLevels.AddRange(levelInfos);

    public void RestartLevel() =>
        OpenPlayView(_currentLevelNumber);

    public void OpenLastPlayView() =>
        OpenPlayView(_lastLevelNumber);

    public void OpenPlayView(int levelNumber)
    {
        if (_energyController.IsCanPlay)
        {
            _currentLevelNumber = levelNumber;
            _database.Load(levelNumber);
            _gameplay.Load(levelNumber);
        }
        else GameEventMessage.SendEvent(OPEN_STORE_EVENT_ID);
    }

    private void PlayLevel(int levelNumber)
    {
        _energyController.SpendPlayCost();
        _gameplay.Begin();
        Analytic.LogStartLevel(levelNumber);
        Try++;
        _startLevelTimestamp = Time.time;
    }

    private void SetupLevels()
    {
        foreach (LevelInfo levelInfo in _allLevels)
        {
            var levelNum = levelInfo.LevelNumber;
            var isCompleted = _lastLevelNumber > 0 && levelNum < LastLevelNum;
            var isUnlocked = levelNum <= LastLevelNum;
            levelInfo.Setup(isUnlocked, isCompleted);
        }
    }

    private void OnGameplayInit(int levelNum, Data[] data)
    {
        _levelStartView.SetLevelName(levelNum);

        _levelStartView.SetPicturesCount(data.Length);
        _levelStartView.SetDifferencesCount(data[0].Points.Length);

        _levelStartView.StartTimer(() => PlayLevel(levelNum));
        _levelStartView.Show();
    }

    #region Save
    private void SaveLastLevel()
    {
        PlayerPrefs.SetInt(LAST_LEVEL_ID, _lastLevelNumber);
    }

    private void LoadLastLevel()
    {
        _lastLevelNumber = PlayerPrefs.GetInt(LAST_LEVEL_ID, 0);
    }
    #endregion

    #region Debug
    [ContextMenu("Complete")]
    private void DebugComplete()
    {
        CompleteLevel(_lastLevelNumber);
    }
    #endregion
}