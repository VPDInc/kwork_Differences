using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class LevelInfo : MonoBehaviour
{
    public int LevelNumber => _levelNumber;
    public EpisodeInfo EpisodeInfo => _episodeInfo;

    public int Estimation => _estimation;

    [SerializeField] private GameObject _completedCupSprite = default;
    [SerializeField] private GameObject _lockedCupSprite = default;
    [SerializeField] private GameObject _avatar = default;
    
    [SerializeField] private TMP_Text _levelNumLabel = default;
    [SerializeField] private GameObject _activeBGSprite = default;
    [SerializeField] private GameObject _completedBGSprite = default;
    [SerializeField] private GameObject _lockedBGSprite = default;

    [SerializeField] private Canvas _avatarCanvas = default;

    [Inject] private UIProfileView _uiProfileView = default;
    [Inject] private Camera _camera = default;
    [Inject] private LevelController _levelController;

    private EpisodeInfo _episodeInfo;
    private int _levelNumber = 0;
    private int _estimation = 0;
    private bool _isCompleted = false;
    private bool _isClick = false;

    private void Awake()
    {        
        _activeBGSprite.SetActive(false);
        _completedBGSprite.SetActive(false);
        _lockedBGSprite.SetActive(true);
        _avatar.SetActive(false);
    }

    private void OnMouseUp()
    {
        if (_isCompleted && _isClick)
            _levelController.OpenPlayView(_levelNumber);
    }

    private void OnMouseDown() =>
        _isClick = true;

    private void OnMouseExit() =>
        _isClick = false;

    public void Init(EpisodeInfo episodeInfo, int levelNumber)
    {
        _avatarCanvas.worldCamera = _camera;
        _episodeInfo = episodeInfo;
        
        _levelNumber = levelNumber;
        _levelNumLabel.text = (levelNumber + 1).ToString();
    }

    public void Setup(bool isUnlocked, bool isCompleted)
    {
        _isCompleted = isCompleted;

        if(isUnlocked) UnlockLevel(true);
        
        if (_isCompleted) CompleteLevel();
        else Lock();
    }

    public void UnlockLevel(bool isInstant)
    {
        _activeBGSprite.SetActive(true);
        _completedBGSprite.SetActive(false);
        _lockedBGSprite.SetActive(false);

        if(!_episodeInfo.IsUnlocked)
            _episodeInfo.UnlockEpisode(isInstant);
    }

    public void CompleteLevel()
    {
        _isCompleted = true;
        _activeBGSprite.SetActive(false);
        _completedBGSprite.SetActive(true);
        _lockedBGSprite.SetActive(false);
        Unlock();
    }

    public void OnProfileClick()
    {
        _uiProfileView.Show(false);
    }

    public void SetAvatar(bool toggle)
    {
        _avatar.SetActive(toggle);
    }
    
    private void Unlock()
    {
        _completedCupSprite.SetActive(true);
        _lockedCupSprite.SetActive(false);
    }

    private void Lock()
    {
        _completedCupSprite.SetActive(false);
        _lockedCupSprite.SetActive(true);
    }
}