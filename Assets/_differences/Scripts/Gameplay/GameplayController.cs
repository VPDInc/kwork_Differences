using System.Collections.Generic;

using UnityEngine;

using Zenject;

public class GameplayController : MonoBehaviour {
    [SerializeField] float _duration = 10f;
    
    bool IsStarted { get; set; }
    
    [Inject] UITimer _timer = default;
    [Inject] UIGameplay _uiGameplay = default;
    [Inject] Database _database = default;
    
    readonly List<Point> _points = new List<Point>();
    
    void Start() {
        _timer.Expired += OnTimerExpired;
        StartGameplay();
    }

    void OnDestroy() {
        _timer.Expired -= OnTimerExpired;
    }

    void Update() {
        if (!IsStarted)
            return;

        var mousePos = Input.mousePosition;
        if (Input.GetMouseButtonDown(0)) {
            if (_uiGameplay.IsOverlap(mousePos, out var point)) {
                _points.Remove(point);
                if (_points.Count == 0) {
                    StopGameplay();
                }
            }
        }
    }

    public void StartGameplay() {
        var levelData = _database.GetLevelByNum(0);
        _points.Clear();
        _points.AddRange(levelData.Points);
        _uiGameplay.Initialize(levelData);
        _timer.Launch(_duration);
        IsStarted = true;
    }

    void StopGameplay() {
        IsStarted = false;
        _timer.Stop();
    }
    
    void OnTimerExpired() {
        StopGameplay();
    }
}
