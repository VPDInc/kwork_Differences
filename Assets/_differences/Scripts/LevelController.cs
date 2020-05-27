using System.Collections.Generic;

using UnityEngine;

public class LevelController : MonoBehaviour {
    public int LastLevel => _lastLevelNum;
    
    int _lastLevelNum = 0;
    List<LevelInfo> _allLevels = new List<LevelInfo>();
    
    const string LAST_LEVEL_ID = "last_level";

    void Start() {
        LoadLastLevel();
    }

    public void SetLastLevel(int num) {
        _lastLevelNum = num;
        SaveLastLevel();
        if(num + 1 < _allLevels.Count)
            _allLevels[num+1].UnlockLevel();
    }

    public void AddLevelToList(IEnumerable<LevelInfo> levelInfos) {
        _allLevels.AddRange(levelInfos);
    }

    void SaveLastLevel() {
        PlayerPrefs.SetInt(LAST_LEVEL_ID, _lastLevelNum);
    }

    void LoadLastLevel() {
        _lastLevelNum = PlayerPrefs.GetInt(LAST_LEVEL_ID);
    }
}