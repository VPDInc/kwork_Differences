using DG.Tweening;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

using Random = UnityEngine.Random;

public class LevelInfo : MonoBehaviour {
    public int LevelNum => _levelNum;
    public int Estimation => _estimation;

    [SerializeField] GameObject _completedCupSprite = default;
    [SerializeField] GameObject _lockedCupSprite = default;
    
    [SerializeField] TMP_Text _levelNumLabel = default;
    [SerializeField] GameObject _activeBGSprite = default;
    [SerializeField] GameObject _completedBGSprite = default;
    [SerializeField] GameObject _lockedBGSprite = default;

    EpisodeInfo _episodeInfo;
    int _levelNum = 0;
    int _estimation = 0;
    bool _isCompleted = false;
    bool _isUnlocked = false;
    EventSystem _eventSystem = default;

    void Awake() {
        _eventSystem = EventSystem.current;
        
        _activeBGSprite.SetActive(false);
        _completedBGSprite.SetActive(false);
        _lockedBGSprite.SetActive(true);
    }

    public void Init(EpisodeInfo episodeInfo, int levelNum) {
        _episodeInfo = episodeInfo;
        
        _levelNum = levelNum;
        _levelNumLabel.text = (levelNum + 1).ToString();
    }

    public void Setup(bool isUnlocked, bool isCompleted) {
        _isCompleted = isCompleted;
        _isUnlocked = isUnlocked;

        if(isUnlocked)
            UnlockLevel(true);
        
        if (_isCompleted) {
            CompleteLevel();
        } else {
            LockVfx(true);
        }
    }

    public void UnlockLevel(bool isInstant) {
        _isUnlocked = true;
        
        _activeBGSprite.SetActive(true);
        _completedBGSprite.SetActive(false);
        _lockedBGSprite.SetActive(false);

        if(!_episodeInfo.IsUnlocked)
            _episodeInfo.UnlockEpisode(isInstant);
    }

    public void CompleteLevel() {
        _isCompleted = true;
        _activeBGSprite.SetActive(false);
        _completedBGSprite.SetActive(true);
        _lockedBGSprite.SetActive(false);
        UnlockVfx(false);
    }

    void UnlockVfx(bool isInstant) {
        _completedCupSprite.SetActive(true);
        _lockedCupSprite.SetActive(false);
    }

    void LockVfx(bool isInstant) {
        _completedCupSprite.SetActive(false);
        _lockedCupSprite.SetActive(true);
    }
}