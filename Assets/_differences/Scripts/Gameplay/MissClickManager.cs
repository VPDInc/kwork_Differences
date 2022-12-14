using UnityEngine;

using Zenject;

public class MissClickManager : MonoBehaviour {
    [SerializeField] float _clickWarnDeltaSec = 5;
    [SerializeField] float[] _reduceTimeLevels = {10, 20, 30};
    [SerializeField] float _clearingDeltaSec = 20;
    [SerializeField] int _clicksToChangeLevel = 3;
    [SerializeField] RectTransform _missingIndicatorPrefab = default;
    [SerializeField] Transform _indicatorContainer = default;
    [SerializeField] Canvas _activeCanvas = default;

    [Inject] UITimer _timer = default;
    [Inject] VibrationManager _vibrationManager = default;
    
    int _currentLevel = 0;
    float _lastClickDeltaTime = 0;
    int _currentClicksInARow = 0;
    
    public void Catch() {
        _vibrationManager.VibratePeek();
        
        _currentClicksInARow++;

        if (Time.time - _lastClickDeltaTime >= _clickWarnDeltaSec)
            _currentClicksInARow = 0;
        
        if (Time.time - _lastClickDeltaTime >= _clearingDeltaSec) {
            _currentLevel = 0;
            _currentClicksInARow = 0;
        }

        if (_currentClicksInARow >= _clicksToChangeLevel) {
            ReduceTime();
            _currentLevel = Mathf.Min(_currentLevel + 1, _reduceTimeLevels.Length - 1);
            _currentClicksInARow = 0;
        }

        _lastClickDeltaTime = Time.time;

        var mousePos = Input.mousePosition;
        var screenPosition = _activeCanvas.ScreenToCanvasPosition(mousePos);
        var indicator = Instantiate(_missingIndicatorPrefab, _indicatorContainer);
        indicator.position = screenPosition;
    }

    public void Reset() {
        _currentClicksInARow = 0;
        _lastClickDeltaTime = 0;
        _currentLevel = 0;
    }

    void ReduceTime() {
        _timer.ReduceTime(_reduceTimeLevels[_currentLevel]);
    }
}
