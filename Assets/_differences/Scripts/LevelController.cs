using System.Collections.Generic;

using UnityEngine;

public class LevelController : MonoBehaviour {
    public int LastLevelNum => _lastLevelNum;
    
    int _lastLevelNum = 0;
    List<LevelInfo> _allLevels = new List<LevelInfo>();
    
    const string LAST_LEVEL_ID = "last_level";
    const string LEVEL_ESTIMATION_PREFIX = "level_estimation_";

    void Start() {
        LoadLastLevel();
        SetupLevels();
    }

    public void CompleteLevel(int num) {
        _lastLevelNum = num;
        var level = _allLevels[num];
        
        SaveLastLevel();
        SaveLevelEstimation(_lastLevelNum, level.Estimation);
        
        if(num + 1 < _allLevels.Count)
            _allLevels[num+1].UnlockLevel(false);
    }

    public void AddLevelToList(IEnumerable<LevelInfo> levelInfos) {
        _allLevels.AddRange(levelInfos);
    }

    void SetupLevels() {
        foreach (LevelInfo levelInfo in _allLevels) {
            var levelNum = levelInfo.LevelNum;
            var isCompleted = levelNum <= LastLevelNum;
            var isUnlocked = levelNum <= LastLevelNum + 1;
            levelInfo.Setup(isUnlocked, isCompleted, LoadLevelEstimation(levelNum));
        }
    }

    void SaveLastLevel() {
        PlayerPrefs.SetInt(LAST_LEVEL_ID, _lastLevelNum);
    }

    void SaveLevelEstimation(int level, int estimation) {
        PlayerPrefs.SetInt(LEVEL_ESTIMATION_PREFIX + level, estimation);
    }

    void LoadLastLevel() {
        _lastLevelNum = PlayerPrefs.GetInt(LAST_LEVEL_ID, -1);
    }

    int LoadLevelEstimation(int level) {
        var estimation = PlayerPrefs.GetInt(LEVEL_ESTIMATION_PREFIX + level);
        return estimation;
    }
}