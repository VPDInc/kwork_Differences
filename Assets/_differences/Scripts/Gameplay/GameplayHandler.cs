using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Zenject;

public class GameplayHandler : MonoBehaviour {
    [SerializeField] float _timePerOneDifference = 20f;
    [SerializeField] float _addTimeAfterOver = 25f;
    [SerializeField] StarsEarningConfig _config = default;
    [SerializeField] UIPictureCountBar _uiPictureCountBar = default;
    
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
    
    bool IsStarted { get; set; }
    readonly List<Point> _pointsRemain = new List<Point>();
    readonly List<PictureResult> _pictureResults = new List<PictureResult>();
    Data[] _levelsData;
    int _currentPictureResult = 0;
    Vector3 _startPos;
    // (Sprite, Sprite)[] _loadedSprites;
    int _levelNum = 0;

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
            
            if (_uiGameplay.IsOverlap(mousePos, out var point)) {
                _pointsRemain.Remove(point);
                _pictureResults[_currentPictureResult].DifferencePoints[point.Number].IsOpen = true;
                UpdateStars(_config.StarsPerFoundDifference);
                
                if (_pointsRemain.Count == 0) {
                    UpdateStars(_config.StarsPerCompletedPicture);
                    _currentPictureResult++;
                    if (_currentPictureResult == _levelsData.Length)
                        StopGameplay(true);
                    else
                        ChangePictures();
                }
            } else {
                _missClickManager.Catch();
            }
        }
    }

    public void Pause() {
        _timer.Pause();
        _middleScreen.Show(() => {
            _pause.Show(_pointsRemain.Count);
        });
    }
    
    public void Exit() {
        StopGameplay(false);
    }

    public void ContinueWithTimeBoost() {
        _middleScreen.Hide(() => {
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

    void StopGameplay(bool isWin) {
        StartCoroutine(Stopping(isWin));
        IsStarted = false;
        _timer.Stop();
    }

    IEnumerator Stopping(bool isWin) {
        yield return new WaitForSeconds(WAIT_BETWEEN_PICTURES_CHANGING);
        _middleScreen.Show(() => {
            _uiGameplay.Complete();
            _missClickManager.Reset();
            _gameplayController.StopLevel(isWin, _pictureResults.ToArray());
            StartCoroutine(WaitAndHideScreen());
        });
    }

    IEnumerator WaitAndHideScreen() {
        yield return new WaitForSeconds(WAIT_BETWEEN_PICTURES_CHANGING);
        _middleScreen.Hide();
    }

    void OnBegan() {
        _middleScreen.Show(FillStartGameplay);
    }
    
    void FillStartGameplay() {
        StartCoroutine(FillGameplayAndStartRoutine());
        _uiPictureCountBar.AddSegment();
    }

    void ChangePictures() {
        StartCoroutine(ChangePicturesRoutine());
    }

    IEnumerator ChangePicturesRoutine() {
        _timer.Pause();
        
        yield return new WaitForSeconds(WAIT_BETWEEN_PICTURES_CHANGING);
        
        IsStarted = false;
        _uiGameplay.Clear();
        _pointsRemain.Clear();
        var levelData = _levelsData[_currentPictureResult];
        var fixedPoints = FixPoints(levelData.Points, _pictureResults[_currentPictureResult].Pictures);
        levelData.Points = fixedPoints;
        _pointsRemain.AddRange(levelData.Points);
        _uiGameplay.Initialize(levelData, _pictureResults[_currentPictureResult].Pictures);
        
        yield return new WaitForSeconds(WAIT_BETWEEN_PICTURES_CHANGING);
        
        _uiPictureCountBar.SetSegmentAmount(_levelsData.Length);
        IsStarted = true;
        _timer.Resume();
    }

    IEnumerator FillGameplayAndStartRoutine() {
        IsStarted = false;
        _uiGameplay.Clear();
        _pointsRemain.Clear();

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

        var levelData = _levelsData[_currentPictureResult];
        

        var fixedPoints = FixPoints(levelData.Points, _pictureResults[_currentPictureResult].Pictures);
        levelData.Points = fixedPoints;
        _pointsRemain.AddRange(levelData.Points);
        
        _uiGameplay.Initialize(levelData, _pictureResults[_currentPictureResult].Pictures);

        _uiPictureCountBar.SetSegmentAmount(_levelsData.Length);
        _uiGameplay.Show();
        
        yield return new WaitForSeconds(1);
        
        _middleScreen.Hide(() => {
            IsStarted = true;
            _timer.Launch(_timePerOneDifference * fixedPoints.Length);
        });
    }

    void OnTimerExpired() {
        StartCoroutine(ShowTipAndShowTimeIsUpMenu());
    }

    IEnumerator ShowTipAndShowTimeIsUpMenu() {
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

    Point[] FixPoints(Point[] points, (Sprite, Sprite) loadedSprite) {
        var width = loadedSprite.Item1.texture.width;
        var height = loadedSprite.Item1.texture.height;
        var fixedPoints = new Point[points.Length];
        for (int i = 0; i < points.Length; i++) {
            fixedPoints[i] = DiffUtils.FixPointRelative(points[i], width, height);
        }

        return fixedPoints;
    }
}
