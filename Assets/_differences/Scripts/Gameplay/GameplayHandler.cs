using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Airion.Audio;

using DG.Tweening;

using UnityEngine;

using Zenject;

public class GameplayHandler : MonoBehaviour {
    public event Action DifferenceFound;
    public event Action GameStarted;
    public event Action PictureChangingStarted;
    public event Action GameEnded;
    
    [SerializeField] float _timePerOneDifference = 20f;
    [SerializeField] float _addTimeAfterOver = 25f;
    [SerializeField] StarsEarningConfig _config = default;
    [SerializeField] UIPictureCountBar _uiPictureCountBar = default;
    [SerializeField] Canvas _activeCanvas = default;
    [SerializeField] CanvasGroup _completeGroup = default;

    [Inject] UITimer _timer = default;
    [Inject] UIGameplay _uiGameplay = default;
    [Inject] GameplayController _gameplayController = default;
    [Inject] MissClickManager _missClickManager = default;
    [Inject] UIAimTip _aimTip = default;
    [Inject] UIStars _uiStars = default;
    [Inject] UIMiddleScreen _middleScreen = default;
    [Inject] UITimeIsUp _timeIsUp = default;
    [Inject] UIPause _pause = default;
    [Inject] Database _database = default;
    [Inject] UIMedalEarningFX _medalEarningFx = default;
    [Inject] ThemeController _themeController = default;
    [Inject] AudioManager _audioManager = default;

    bool IsStarted { get; set; }
    readonly List<PictureResult> _pictureResults = new List<PictureResult>();
    Data[] _levelsData;
    int _currentPictureResult = 0;
    Vector3 _startPos;
    int _levelNum = 0;
    float _lastDiffTimestamp;

    const float SWIPE_DETECTION_LEN = 20;
    const float WAIT_BETWEEN_PICTURES_CHANGING = 1.5f;
    
    void Start() {
        _timer.Expired += OnTimerExpired;
        _gameplayController.Began += OnBegan;
        _gameplayController.Initialized += OnInitialized;
    }

    void OnDestroy() {
        _timer.Expired -= OnTimerExpired;
        _gameplayController.Began -= OnBegan;
        _gameplayController.Initialized -= OnInitialized;
    }
    
    void Update() {
        if (!IsStarted)
            return;
        
        if (Input.GetMouseButtonDown(0))
            _startPos = Input.mousePosition;

        var mousePos = Input.mousePosition;
        if (Input.GetMouseButtonUp(0)) {
            if ((_startPos - Input.mousePosition).magnitude > SWIPE_DETECTION_LEN)
                return;
            
            if (!_uiGameplay.IsOverImage(mousePos))
                return;

            var overlapStatus = _uiGameplay.TryOverlap(mousePos, out var point);
            if (overlapStatus == UIGameplay.OverlapStatus.Found) {
                _audioManager.PlayOnce("diff-found");
                var screenPosition = _activeCanvas.ScreenToCanvasPosition(mousePos);
                _medalEarningFx.CallEffect(screenPosition, _config.StarsPerFoundDifference);
                
                Analytic.DiffFound(LevelController.GetLastLevelNum(), _levelsData[_currentPictureResult].Id, point.Number, _pictureResults[_currentPictureResult].DifferencePoints.Length -  _uiGameplay.ClosedPoints.Length, Time.time - _lastDiffTimestamp);
                
                _lastDiffTimestamp = Time.time;
                DifferenceFound?.Invoke();
                
                _pictureResults[_currentPictureResult].DifferencePoints[point.Number].IsOpen = true;
                UpdateStars(_config.StarsPerFoundDifference);
                
                if (_uiGameplay.ClosedPoints.Length == 0) {
                    var seq = DOTween.Sequence();
                    seq.Append(_completeGroup.DOFade(1, WAIT_BETWEEN_PICTURES_CHANGING));
                    seq.AppendInterval(WAIT_BETWEEN_PICTURES_CHANGING);
                    seq.Append(_completeGroup.DOFade(0, WAIT_BETWEEN_PICTURES_CHANGING));
                    
                    _completeGroup.DOFade(1, WAIT_BETWEEN_PICTURES_CHANGING);
                    UpdateStars(_config.StarsPerCompletedPicture);
                    _currentPictureResult++;
                    _audioManager.PlayOnce("win");
                    if (_currentPictureResult == _levelsData.Length)
                        StopGameplay(true, true);
                    else
                        ChangePictures();
                }
            } else if (overlapStatus == UIGameplay.OverlapStatus.NotFound) {
                _missClickManager.Catch();
                _audioManager.PlayOnce("error");
            }
        }
    }

    public void Pause() {
        _timer.Pause();
        _middleScreen.Show(() => {
            _pause.Show(_uiGameplay.ClosedPoints.Length);
        });
    }
    
    public void Exit() {
        StopGameplay(false, false);
    }

    public void ContinueWithTimeBoost() {
        _middleScreen.Hide(() => {
            IsStarted = true;
            _timer.Launch(_addTimeAfterOver);
        });
    }

    public void Continue() {
        _middleScreen.Hide(() => {
            _timer.Resume();
        });
    }

    void UpdateStars(int amount) {
        _uiStars.Add(amount);
        var diff = _pictureResults[_currentPictureResult];
        diff.StarsCollected += amount;
        _pictureResults[_currentPictureResult] = diff;
    }

    void StopGameplay(bool isWin, bool withDelay) {
        GameEnded?.Invoke();
        StartCoroutine(Stopping(isWin, withDelay));
        IsStarted = false;
        _timer.Stop();
    }

    IEnumerator Stopping(bool isWin, bool withDelay) {
        if (withDelay)
            yield return new WaitForSeconds(WAIT_BETWEEN_PICTURES_CHANGING);
        
        _middleScreen.Show(() => {
            _uiGameplay.Complete();
            _missClickManager.Reset();
            _gameplayController.StopLevel(isWin, _pictureResults.ToArray());
            StartCoroutine(WaitAndHideScreen());
        });
    }

    IEnumerator WaitAndHideScreen() {
        yield return new WaitForSeconds(WAIT_BETWEEN_PICTURES_CHANGING * 0.5f);
        _middleScreen.Hide(() => {
            _themeController.PlayMainTheme();
        });
    }

    void OnBegan() {
        _themeController.StopTheme();
        _middleScreen.Show(FillStartGameplay);
    }
    
    void FillStartGameplay() {
        StartCoroutine(FillGameplayAndStartRoutine());
    }

    void ChangePictures() {
        StartCoroutine(ChangePicturesRoutine());
    }

    IEnumerator ChangePicturesRoutine() {
        PictureChangingStarted?.Invoke();
        _timer.Pause();
        
        yield return new WaitForSeconds(WAIT_BETWEEN_PICTURES_CHANGING);
        
        IsStarted = false;
        _uiGameplay.Clear();
        var levelData = _levelsData[_currentPictureResult];
        _uiGameplay.SwitchData(levelData, _pictureResults[_currentPictureResult].Pictures);
        _uiPictureCountBar.AddSegment();        
        
        yield return new WaitForSeconds(WAIT_BETWEEN_PICTURES_CHANGING);
        
        IsStarted = true;
        _timer.Resume();
    }

    IEnumerator FillGameplayAndStartRoutine() {
        IsStarted = false;
        _uiGameplay.Clear();
        
        yield return new WaitWhile(() => _database.GetLoadingStatus(_levelNum) != Database.LoadingStatus.Success);
        
        var loadedSprites = _database.GetPictures(_levelNum);

        for (var index = 0; index < _levelsData.Length; index++) {
            var data = _levelsData[index];
            var points = new DifferencePoint[data.Points.Length];
            for (int i = 0; i < data.Points.Length; i++) {
                points[i].IsOpen = false;
            }

            _pictureResults.Add(new PictureResult() {
                DifferencePoints = points,
                Orientation = data.Orientation,
                Picture1 = loadedSprites[index].Item1,
                Picture2 = loadedSprites[index].Item2
            });
        }

        _uiPictureCountBar.SetSegmentAmount(_levelsData.Length);
        _uiGameplay.Show();
        
        _uiGameplay.StartWithData(_levelsData, _pictureResults.Select(p => p.Pictures).ToArray(),
            () => {
                IsStarted = true;
                _timer.Launch(_timePerOneDifference * _levelsData[_currentPictureResult].Points.Length);
                _lastDiffTimestamp = Time.time;
                _uiPictureCountBar.AddSegment();
                GameStarted?.Invoke();
            });
        
        yield return new WaitForSeconds(1);
        
        _middleScreen.Hide(() => {
            _themeController.PlayGameplayTheme();
        });
    }

    void OnTimerExpired() {
        StartCoroutine(ShowTipAndShowTimeIsUpMenu());
    }

    IEnumerator ShowTipAndShowTimeIsUpMenu() {
        IsStarted = false;
        _aimTip.ShowTip();
        yield return new WaitForSeconds(1.5f);
        _middleScreen.Show((() => {
            _timeIsUp.Show(_addTimeAfterOver, _pictureResults.Sum(res => res.StarsCollected));
        }));
    }
    
    void OnInitialized(int levelNum, Data[] levelsData) {
        _levelNum = levelNum;
        _levelsData = levelsData;
        
        _pictureResults.Clear();
        _uiStars.Reset();
        
        _currentPictureResult = 0;
    }
}
