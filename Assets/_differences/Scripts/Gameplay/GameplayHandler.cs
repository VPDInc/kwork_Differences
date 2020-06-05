using System.Collections.Generic;

using Doozy.Engine.UI;

using UnityEngine;

using Zenject;

public class GameplayHandler : MonoBehaviour {
    [SerializeField] float _duration = 10f;
    [SerializeField] float _addTimeAfterOver = 25f;
    [SerializeField] UIView _timeExpiredView = default;
    
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
        _timeExpiredView.Hide(true);
        _levelsData = levelsData;
        //TODO: Start pictures loading
    }

    public void ExitClick() {
        StopGameplay(false);
    }

    public void AddTimeClick() {
        _timer.Launch(_addTimeAfterOver);
        _aimTip.ShowTip();
        _timeExpiredView.Hide();
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
        
        FillGameplay(_levelsData[_currentPictureResult]);
        IsStarted = true;
        _timer.Launch(_duration);
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
                    _currentPictureResult++;
                    if (_currentPictureResult == _levelsData.Length)
                        StopGameplay(true);
                    else
                        FillGameplay(_levelsData[_currentPictureResult]);
                }
            } else {
                _missClickManager.Catch();
            }
        }
    }

    void FillGameplay(Data levelData) {
        _uiGameplay.Clear();
        _points.Clear();
        _points.AddRange(levelData.Points);
        _uiGameplay.Initialize(levelData);
    }

    void StopGameplay(bool isWin) {
        _missClickManager.Reset();
        IsStarted = false;
        _timer.Stop();
        _uiGameplay.Complete();
        _gameplayController.StopLevel(isWin, _pictureResults.ToArray());
    }
    
    void OnTimerExpired() {
        _timeExpiredView.Show();
    }
}
