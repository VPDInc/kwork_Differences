using DG.Tweening;

using TMPro;

using UnityEngine;

using Zenject;

using Random = UnityEngine.Random;

public class LevelInfo : MonoBehaviour {
    public bool IsCompleted => _isCompleted;
    public int LevelNum => _levelNum;
    public int Estimation => _estimation;

    [SerializeField] TMP_Text _levelNumLabel = default;
    [SerializeField] SpriteRenderer[] _spritesToHide = default;

    [Inject] LevelController _levelController = default;

    StarHandler _starHandler;
    EpisodeInfo _episodeInfo;
    int _levelNum = 0;
    int _estimation = 0;
    bool _isCompleted = false;
    bool _isUnlocked = false;

    const float LOCKED_ALPHA = 0.75f;
    const float VFX_DURATION = 0.5f;

    void Awake() {
        _starHandler = GetComponentInChildren<StarHandler>();
    }

    void Start() {
        ShowStarHandler(true);
    }

    public void Init(EpisodeInfo episodeInfo, int levelNum) {
        _episodeInfo = episodeInfo;
        
        _levelNum = levelNum;
        _levelNumLabel.text = (levelNum + 1).ToString();
    }

    public void Setup(bool isUnlocked, bool isCompleted, int stars = 0) {
        _isCompleted = isCompleted;
        _isUnlocked = isUnlocked;
        
        SetStars(stars, true);
        
        if(isUnlocked)
            UnlockLevel(true);
        
        if (_isCompleted) {
            UnlockVfx(true);
        } else {
            LockVfx(true);
        }
        
        
    }

    public void UnlockLevel(bool isInstant) {
        _isUnlocked = true;
        ShowStarHandler(isInstant);
        
        if(!_episodeInfo.IsUnlocked)
            _episodeInfo.UnlockEpisode(isInstant);
    }

    public void CompleteLevel() {
        _isCompleted = true;
        ShowStarHandler(false);
        SetStars(Random.Range(1, 4), false);
        UnlockVfx(false);
        _levelController.CompleteLevel(_levelNum);
    }
    
    public void ShowStarHandler(bool isInstant) {
        _starHandler.Show(_isUnlocked, isInstant);
    }

    public void SetStars(int stars, bool isInstant) {
        _estimation = stars;
        _starHandler.SetStars(stars, isInstant);
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

    void OnMouseDown() {
        if(_isCompleted || !_isUnlocked) return;
        
        CompleteLevel();
    }
}