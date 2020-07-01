using System.Collections;
using System.Collections.Generic;

using Airion.Currency;

using UnityEngine;

using Zenject;

public class GameplayHandler : MonoBehaviour {
    public bool IsEnoughToAddTime => _soft.IsEnough(_addTimeCost);
    
    [SerializeField] float _duration = 10f;
    [SerializeField] float _addTimeAfterOver = 25f;
    [SerializeField] int _addTimeCost = 900;
    [SerializeField] StarsEarningConfig _config = default;
    
    [Inject] UITimer _timer = default;
    [Inject] UIGameplay _uiGameplay = default;
    [Inject] GameplayController _gameplayController = default;
    [Inject] MissClickManager _missClickManager = default;
    [Inject] UIAimTip _aimTip = default;
    [Inject] CurrencyManager _currencyManager = default;
    [Inject] UIStars _uiStars = default;
    
    bool IsStarted { get; set; }
    readonly List<Point> _points = new List<Point>();
    readonly List<PictureResult> _pictureResults = new List<PictureResult>();
    Data[] _levelsData;
    int _currentPictureResult = 0;
    Vector3 _startPos;
    (Sprite, Sprite)[] _loadedSprites;
    Coroutine _spriteLoaderRoutine;
    Coroutine _gameplayFillingRoutine;
    Currency _soft;

    const float SWIPE_DETECTION_LEN = 20;
    
    void Start() {
        _timer.Expired += OnTimerExpired;
        _gameplayController.Began += OnBegan;
        _gameplayController.Initialized += OnInitialized;
        _soft = _currencyManager.GetCurrency("Soft");
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
                _points.Remove(point);
                _pictureResults[_currentPictureResult].DifferencePoints[point.Number].IsOpen = true;
                UpdateStars(_config.StarsPerFoundDifference);
                
                if (_points.Count == 0) {
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

    void UpdateStars(int amount) {
        _uiStars.Add(amount);
        var diff = _pictureResults[_currentPictureResult];
        diff.StarsCollected += amount;
        _pictureResults[_currentPictureResult] = diff;
    }

    public void ExitClick() {
        StopGameplay(false);
    }

    public void AddTimeClick() {
        _timer.Launch(_addTimeAfterOver);
        _aimTip.ShowTip();
        _uiGameplay.HideTimeExpired();
        _soft.Spend(_addTimeCost);
    }
    
    void FillGameplay() {
        if (_gameplayFillingRoutine != null) 
            StopCoroutine(_gameplayFillingRoutine);
        _gameplayFillingRoutine = StartCoroutine(FillAndStartGameplay());
    }

    void StopGameplay(bool isWin) {
        _missClickManager.Reset();
        IsStarted = false;
        _timer.Stop();
        _uiGameplay.Complete();
        _gameplayController.StopLevel(isWin, _pictureResults.ToArray());
    }

    void OnBegan() {
        _timer.Launch(_duration);
        _uiGameplay.Show();
        FillGameplay();
    }

    void OnTimerExpired() {
        _uiGameplay.ShowTimeExpired();
    }
    
    void OnInitialized(int levelNum, Data[] levelsData) {
        _uiGameplay.HideTimeExpired(true);
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
    
    IEnumerator FillAndStartGameplay() {
        _timer.Pause();
        IsStarted = false;
        
        var levelData = _levelsData[_currentPictureResult];
        _uiGameplay.Clear();
        _points.Clear();

        if (!IsCurrentSpritesLoaded)
            _uiGameplay.ShowWaitWindow();
        
        yield return new WaitWhile(()=> !IsCurrentSpritesLoaded);

        var fixedPoints = FixPoints(levelData.Points, _loadedSprites[_currentPictureResult]);
        levelData.Points = fixedPoints;
        _points.AddRange(levelData.Points);
        
        _uiGameplay.HideWaitWindow();

        _uiGameplay.Initialize(levelData, _loadedSprites[_currentPictureResult]);

        IsStarted = true;
        _timer.Resume();
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
