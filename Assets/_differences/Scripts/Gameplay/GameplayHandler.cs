using System.Collections.Generic;

using UnityEngine;

using Zenject;

public class GameplayHandler : MonoBehaviour {
    [SerializeField] float _duration = 10f;
    
    [Inject] UITimer _timer = default;
    [Inject] UIGameplay _uiGameplay = default;
    [Inject] GameplayController _gameplayController = default;
    
    bool IsStarted { get; set; }
    readonly List<Point> _points = new List<Point>();
    readonly List<PictureResult> _pictureResults = new List<PictureResult>();
    Data[] _levelsData;
    int _currentPictureResult = 0;
    
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
        _levelsData = levelsData;
        //TODO: Start pictures loading
    }

    void OnBegan() {
        // TODO: show view
        // if pucteures not loading
        //    show blocker
        //     wait
        
        
        _pictureResults.Clear();
        _currentPictureResult = 0;
        foreach (var data in _levelsData) {
            var points = new DifferencePoint[data.Points.Length];
            for (int i = 0; i < data.Points.Length; i++) {
                points[i].IsOpen = false;
            } 
            
            _pictureResults.Add(new PictureResult() {
                // TODO: Load sprite
                DifferencePoints = points
            });
        }
        
        StartGameplay(_levelsData[_currentPictureResult]);
    }

    void Update() {
        if (!IsStarted)
            return;

        var mousePos = Input.mousePosition;
        if (Input.GetMouseButtonDown(0)) {
            if (_uiGameplay.IsOverlap(mousePos, out var point)) {
                _points.Remove(point);
                _pictureResults[_currentPictureResult].DifferencePoints[point.Number].IsOpen = true;
                if (_points.Count == 0) {
                    // TODO: Switch level
                    StopGameplay(true);
                }
            }
        }
    }

    void StartGameplay(Data levelData) {
        //Clear old points
        _points.Clear();
        _points.AddRange(levelData.Points);
        _uiGameplay.Initialize(levelData);
        // TODO: Start it only after images loading in UiGameplay
        _timer.Launch(_duration);
        IsStarted = true;
        // TODO: Show ui and block if need
        // TODO: Coroutine
    }

    void StopGameplay(bool isWin) {
        IsStarted = false;
        _timer.Stop();
        _uiGameplay.Complete();
        _gameplayController.StopLevel(isWin, _pictureResults.ToArray());
    }
    
    void OnTimerExpired() {
        StopGameplay(false);
    }
}
