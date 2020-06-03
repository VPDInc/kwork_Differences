using DG.Tweening;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

using Random = UnityEngine.Random;

public class LevelInfo : MonoBehaviour {
    public int LevelNum => _levelNum;
    public int Estimation => _estimation;

    [SerializeField] TMP_Text _levelNumLabel = default;
    [SerializeField] GameObject _activeSprite = default;
    [SerializeField] SpriteRenderer[] _spritesToHide = default;

    [Inject] LevelController _levelController = default;
    
    EpisodeInfo _episodeInfo;
    int _levelNum = 0;
    int _estimation = 0;
    bool _isCompleted = false;
    bool _isUnlocked = false;
    EventSystem _eventSystem = default;

    const float LOCKED_ALPHA = 0.75f;
    const float VFX_DURATION = 0.5f;

    void Awake() {
        _eventSystem = EventSystem.current;
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
            // UnlockVfx(true);
        } else {
            LockVfx(true);
        }
    }

    public void PlayLevel() {
        //DUMMY
        // CompleteLevel();
    }

    public void UnlockLevel(bool isInstant) {
        _isUnlocked = true;
        _activeSprite.SetActive(true);

        if(!_episodeInfo.IsUnlocked)
            _episodeInfo.UnlockEpisode(isInstant);
    }

    public void CompleteLevel() {
        _isCompleted = true;
        _activeSprite.SetActive(false);
        UnlockVfx(false);
        _levelController.CompleteLevel(_levelNum);
    }

    void UnlockVfx(bool isInstant) {
        foreach (SpriteRenderer spriteRenderer in _spritesToHide) {
            spriteRenderer.DOFade(1, isInstant ? 0 : VFX_DURATION);
        }
    }

    void LockVfx(bool isInstant) {
        foreach (SpriteRenderer spriteRenderer in _spritesToHide) {
            spriteRenderer.DOFade(LOCKED_ALPHA, isInstant ? 0 : VFX_DURATION);
        }
    }

    // void OnMouseDown() {
    //     if(!_isUnlocked || _eventSystem.IsPointerOverGameObject()) return;
    //     _levelController.OpenPlayView(_levelNum);
    // }
}