using System.Collections.Generic;

using Lean.Touch;

using UnityEngine;

using Zenject;

public class LevelController : MonoBehaviour {
    public int LastLevelNum => _lastLevelNum;

    [Inject] LeanDragCamera _leanDragCamera = default;
    [Inject] UILevelStartView _levelStartView = default;
    
    int _lastLevelNum = 0;
    List<LevelInfo> _allLevels = new List<LevelInfo>();
    
    const string LAST_LEVEL_ID = "last_level";

    void Start() {
        LoadLastLevel();
        SetupLevels();
        _leanDragCamera.MoveTo(_allLevels[Mathf.Clamp(_lastLevelNum, 0, _allLevels.Count-1)].transform.position, true);
    }

    public void CompleteLevel(int num) {
        if(num >= _lastLevelNum)
            _lastLevelNum = num + 1;
        var level = _allLevels[Mathf.Clamp(num, 0, _allLevels.Count-1)];
        _leanDragCamera.MoveTo(level.transform.position, false);
        SaveLastLevel();

        if(num + 1 < _allLevels.Count)
            _allLevels[num+1].UnlockLevel(false);
    }

    public void AddLevelToList(IEnumerable<LevelInfo> levelInfos) {
        _allLevels.AddRange(levelInfos);
    }

    public void OpenLastPlayView() {
        OpenPlayView(_lastLevelNum);
    }

    public void OpenPlayView(int levelNum) {
        _levelStartView.SetLevelName(levelNum);
        
        //DUMMY
        _levelStartView.SetPicturesCount(Random.Range(1,3));
        _levelStartView.SetDifferencesCount(Random.Range(5, 9));
        _levelStartView.SetPlayerName("Babaduk");
        //
        
        _levelStartView.StartTimer(()=>PlayLevel(levelNum));
        _levelStartView.Show();
    }
    
    void PlayLevel(int levelNum) {
        //DUMMY
        _allLevels[Mathf.Clamp(levelNum, 0, _allLevels.Count-1)].PlayLevel();
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

}