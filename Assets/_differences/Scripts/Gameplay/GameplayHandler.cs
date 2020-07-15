﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Zenject;

public class GameplayHandler : MonoBehaviour {
    [SerializeField] float _duration = 10f;
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
    
    bool IsStarted { get; set; }
    readonly List<Point> _pointsRemain = new List<Point>();
    readonly List<PictureResult> _pictureResults = new List<PictureResult>();
    Data[] _levelsData;
    int _currentPictureResult = 0;
    Vector3 _startPos;
    (Sprite, Sprite)[] _loadedSprites;
    Coroutine _spriteLoaderRoutine;
    Coroutine _gameplayFillingRoutine;

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
                        FillGameplay();
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
            _middleScreen.Hide();
        });
    }

    void OnBegan() {
        _middleScreen.Show(() => {
            _timer.Launch(_duration);
            _uiGameplay.Show();
            _uiPictureCountBar.SetSegmentAmount(_levelsData.Length);
            FillGameplay();
        });
    }
    
    void FillGameplay() {
        if (_gameplayFillingRoutine != null) 
            StopCoroutine(_gameplayFillingRoutine);
        _gameplayFillingRoutine = StartCoroutine(FillAndStartGameplay());
        _uiPictureCountBar.AddSegment();
    }
    
    IEnumerator FillAndStartGameplay() {
        _timer.Pause();
        IsStarted = false;
        
        if (!_middleScreen.IsShowing)
            yield return new WaitForSeconds(WAIT_BETWEEN_PICTURES_CHANGING);
        
        var levelData = _levelsData[_currentPictureResult];
        _uiGameplay.Clear();
        _pointsRemain.Clear();

        if (!IsCurrentSpritesLoaded)
            _uiGameplay.ShowWaitWindow();

        yield return new WaitWhile(()=> !IsCurrentSpritesLoaded);

        var fixedPoints = FixPoints(levelData.Points, _loadedSprites[_currentPictureResult]);
        levelData.Points = fixedPoints;
        _pointsRemain.AddRange(levelData.Points);
        
        _uiGameplay.HideWaitWindow();

        _uiGameplay.Initialize(levelData, _loadedSprites[_currentPictureResult]);

        if (_middleScreen.IsShowing) {
            yield return new WaitForSeconds(WAIT_BETWEEN_PICTURES_CHANGING);
            _middleScreen.Hide(() => {
                IsStarted = true;
                _timer.Resume();
            });
        } else {
            yield return new WaitForSeconds(WAIT_BETWEEN_PICTURES_CHANGING);
            IsStarted = true;
            _timer.Resume();
        }
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
        _levelsData = levelsData;
        
        _pictureResults.Clear();
        _uiStars.Reset();
        
        _currentPictureResult = 0;
        foreach (var data in _levelsData) {
            var points = new DifferencePoint[data.Points.Length];
            for (int i = 0; i < data.Points.Length; i++) {
                points[i].IsOpen = false;
            } 
            
            _pictureResults.Add(new PictureResult() {
                DifferencePoints = points
            });
        }
        
        if (_spriteLoaderRoutine != null)
            StopCoroutine(_spriteLoaderRoutine);
        _spriteLoaderRoutine = StartCoroutine(LoadSprites());
    }

    IEnumerator LoadSprites() {
        var loader = new Loader(this);
        _loadedSprites = new (Sprite, Sprite)[_levelsData.Length];
        for (var index = 0; index < _levelsData.Length; index++) {
            var data = _levelsData[index];
            var path1 = data.Image1Path;
            var path2 = data.Image2Path;
            yield return loader.Run(path1, path2, data.Storage);
            _loadedSprites[index].Item1 = loader.Result.Item1;
            _loadedSprites[index].Item2 = loader.Result.Item2;

            var diff = _pictureResults[index];
            diff.Picture = _loadedSprites[index].Item1;
            _pictureResults[index] = diff;
        }
    }

    bool IsCurrentSpritesLoaded => _loadedSprites[_currentPictureResult].Item1 != null &&
                                   _loadedSprites[_currentPictureResult].Item2 != null;
    
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
