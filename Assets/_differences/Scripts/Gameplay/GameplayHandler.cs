using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

public class GameplayHandler : MonoBehaviour {
    [SerializeField] float _duration = 10f;
    [SerializeField] float _addTimeAfterOver = 25f;
    
    [Inject] UITimer _timer = default;
    [Inject] UIGameplay _uiGameplay = default;
    [Inject] GameplayController _gameplayController = default;
    [Inject] MissClickManager _missClickManager = default;
    [Inject] UIAimTip _aimTip = default;
    
    bool IsStarted { get; set; }
    readonly List<Point> _points = new List<Point>();
    readonly List<PictureResult> _pictureResults = new List<PictureResult>();
    Data[] _levelsData;
    int _currentPictureResult = 0;
    Vector3 _startPos;
    (Sprite, Sprite)[] _loadedSprites;
    Coroutine _spriteLoaderRoutine;
    Coroutine _gameplayFillingRoutine;

    const float SWIPE_DETECTION_LEN = 20;
    
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

    void OnInitialized(Data[] levelsData) {
        _uiGameplay.HideTimeExpired(true);
        _levelsData = levelsData;
        
        if (_spriteLoaderRoutine != null)
            StopCoroutine(_spriteLoaderRoutine);
        _spriteLoaderRoutine = StartCoroutine(LoadSprites());
    }

    IEnumerator LoadSprites() {
        _loadedSprites = new (Sprite, Sprite)[_levelsData.Length];
        for (var index = 0; index < _levelsData.Length; index++) {
            var data = _levelsData[index];
            var async1 = Resources.LoadAsync<Sprite>("Images/" + data.Image1Path);
            var async2 = Resources.LoadAsync<Sprite>("Images/" + data.Image2Path);
            yield return new WaitWhile(() => !async1.isDone && !async2.isDone);
            _loadedSprites[index].Item1 = (Sprite) async1.asset;
            _loadedSprites[index].Item2 = (Sprite) async2.asset;

            var diff = _pictureResults[index];
            diff.Picture = _loadedSprites[index].Item1;
            _pictureResults[index] = diff;
        }
    }

    public void ExitClick() {
        StopGameplay(false);
    }

    public void AddTimeClick() {
        _timer.Launch(_addTimeAfterOver);
        _aimTip.ShowTip();
        _uiGameplay.HideTimeExpired();
    }

    void OnBegan() {
        _pictureResults.Clear();
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
        
        _timer.Launch(_duration);
        _uiGameplay.Show();
        FillGameplay();
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
                if (_points.Count == 0) {
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

    void FillGameplay() {
        if (_gameplayFillingRoutine != null) 
            StopCoroutine(_gameplayFillingRoutine);
        _gameplayFillingRoutine = StartCoroutine(FillAndStartGameplay());
    }

    bool IsCurrentSpritesLoaded => _loadedSprites[_currentPictureResult].Item1 != null &&
                                    _loadedSprites[_currentPictureResult].Item2 != null;
    
    IEnumerator FillAndStartGameplay() {
        _timer.Pause();
        IsStarted = false;

        
        var levelData = _levelsData[_currentPictureResult];
        _uiGameplay.Clear();
        _points.Clear();
        _points.AddRange(levelData.Points);



        if (!IsCurrentSpritesLoaded)
            _uiGameplay.ShowWaitWindow();
        
        yield return new WaitWhile(()=> !IsCurrentSpritesLoaded);
        
        _uiGameplay.HideWaitWindow();

        _uiGameplay.Initialize(levelData, _loadedSprites[_currentPictureResult]);

        IsStarted = true;
        _timer.Resume();
    }

    void StopGameplay(bool isWin) {
        _missClickManager.Reset();
        IsStarted = false;
        _timer.Stop();
        _uiGameplay.Complete();
        _gameplayController.StopLevel(isWin, _pictureResults.ToArray());
    }
    
    void OnTimerExpired() {
        _uiGameplay.ShowTimeExpired();
    }
}
