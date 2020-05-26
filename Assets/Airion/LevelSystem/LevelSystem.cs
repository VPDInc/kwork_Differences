using System;

using Airion.Extensions;

using UnityEngine;

using Random = UnityEngine.Random;

public class LevelSystem : Singleton<LevelSystem> {
    public event Action<Level> LevelLoaded;
    public int LevelNum {get; private set;}
    public string LevelNumPretty => (LevelNum + 1).ToString();
    public Level Current => _levels[_rawLevelNum];

    public Level Next {
        get {
            var nextLevelNum = _rawLevelNum + 1;
            if (nextLevelNum > _levels.Length - 1) {
                nextLevelNum = GetLevelNumByRepeatingType(nextLevelNum);
            }

            return _levels[nextLevelNum];
        }
    }
    
    public int RawLevelNum => _rawLevelNum;
    
    [SerializeField] bool _isDebugEnabled = false;
    [SerializeField] Level _debugLevel = default;
    [SerializeField] LevelRepeatingType _levelRepeating = LevelRepeatingType.Restart;
    [SerializeField] LevelNumRepeatingType _levelNumRepeating = LevelNumRepeatingType.Count;
    [SerializeField] Level[] _levels = default;
    [SerializeField] int _repeatLast = 10;

    int _rawLevelNum = 0;

    const string SAVED_LEVEL_RAW_NUM_PREFIX = "saved_level_raw";
    const string SAVED_LEVEL_NUM_PREFIX = "saved_level_num";
    
    enum LevelRepeatingType {
        Restart, 
        Last,
        Random
    }
    
    enum LevelNumRepeatingType {
        Restart, 
        Count
    }

    protected override void Awake() {
        base.Awake();
        LoadSaving();
        Initialize();
    }

    void Initialize() {
        Debug.Assert(_levels != null && _levels.Length > 0, $"[{GetType()}] No one level found");
        for (int i = 0; i < _levels.Length; i++) {
            var level = _levels[i];
            _levels[i]= Instantiate(level);
        }
        _repeatLast = Mathf.Min(_repeatLast, _levels.Length);
    }
    
    [ContextMenu("Load Next")]
    public void LoadNext() {
        _rawLevelNum += 1;
        LevelNum = LevelNum + 1;
        
        if (_rawLevelNum > _levels.Length - 1) {
            _rawLevelNum = GetLevelNumByRepeatingType(_rawLevelNum);
            LevelNum = _levelNumRepeating == LevelNumRepeatingType.Count ? LevelNum : LevelNum % _levels.Length;
        }

        CreateLevel(_rawLevelNum);
        SaveCurrent();
    }
    
    [ContextMenu("Load Prev")]
    public void LoadPrev() {
        _rawLevelNum = Mathf.Max(_rawLevelNum - 1, 0);
        LevelNum = Mathf.Max(LevelNum - 1, 0);
        
        CreateLevel(_rawLevelNum);
        SaveCurrent();
    }

    [ContextMenu("Reload")]
    public void ReloadCurrent() {
        Load(_rawLevelNum);
    }

    public void ReloadCurrentFromBeginStage() {
        Load(_rawLevelNum);
    }

    int GetLevelNumByRepeatingType(int nextIndex) {
        switch (_levelRepeating) {
            case LevelRepeatingType.Last:
                var offset = Mathf.Repeat(nextIndex, _repeatLast);
                var startFrom = _levels.Length - _repeatLast;
                return startFrom + (int)offset;
            case LevelRepeatingType.Random:
                return Random.Range(0, _levels.Length);
            default:
                var repeatedIndex = Mathf.Repeat(nextIndex, _levels.Length);
                return (int)repeatedIndex;
        }
    }

    void CreateLevel(int level) {
        if (_isDebugEnabled) {
            LevelLoaded?.Invoke(_debugLevel);
            return;
        }
        
        _rawLevelNum = level;
        LevelLoaded?.Invoke(Current);
    }

    void Load(int levelNum) {
        if (levelNum < 0 || levelNum > _levels.Length) {
            CreateLevel(0);
            return;
        }
        
        CreateLevel(levelNum);
    }

    void LoadSaving() {
        _rawLevelNum = PlayerPrefs.GetInt(SAVED_LEVEL_RAW_NUM_PREFIX, 0);
        LevelNum = PlayerPrefs.GetInt(SAVED_LEVEL_NUM_PREFIX, 0);
    }
    
    public void LoadLast() {
        Load(_rawLevelNum);
    }

    void SaveCurrent() {
        PlayerPrefs.SetInt(SAVED_LEVEL_RAW_NUM_PREFIX, _rawLevelNum);
        PlayerPrefs.SetInt(SAVED_LEVEL_NUM_PREFIX, LevelNum);
    }
}
