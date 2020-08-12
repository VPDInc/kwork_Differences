using System;

using DG.Tweening;

using TMPro;

using UnityEngine;

using Zenject;

public class UITimerUpdater : MonoBehaviour {
    [SerializeField] TextMeshProUGUI _timerText = default;
    [SerializeField] SimpleRotator _rotator = default;
    [SerializeField] float _criticalSecondsRemaining = 10;
    [SerializeField] Color _blinkColor = Color.red;

    [Inject] UITimer _timer = default;

    bool _isCritical = false;
    Color _startColor;
    float _lastTime;

    const string TIMER_FORMAT = "mm\\:ss";

    void Start() {
        _startColor = _timerText.color;
        _rotator.SetSpeed(0);
        _timer.TimerUpdated += OnTimerUpdated;
        _timer.Started += OnResume;
        _timer.Stopped += OnStop;
    }

    void OnDestroy() {
        _timer.TimerUpdated -= OnTimerUpdated;
        _timer.Started -= OnResume;
        _timer.Stopped -= OnStop;
    }

    void OnStop() {
        _rotator.SetSpeed(0);
    }

    void OnResume() {
        _rotator.RestoreSpeed();
    }

    void OnTimerUpdated(float time) {
        UpdateTimer(time);
    }
    
    void UpdateTimer(float time) {
        HandleBlink(time);

        if (TimeSpan.FromSeconds(time).Seconds == TimeSpan.FromSeconds(_lastTime).Seconds)
            return;

        DOTween.Kill(this);
        DOTween.To(() => _lastTime, x => {
            _lastTime = x;
            _timerText.text = TimeSpan.FromSeconds(_lastTime).ToString(TIMER_FORMAT);
        }, time, 0.35f).SetId(this);
    }

    void HandleBlink(float time) {
        if (Mathf.CeilToInt(time) <= _criticalSecondsRemaining) {
            if (!_isCritical) {
                _isCritical = true;
                DOTween.Kill(_timerText);
                _timerText.transform.DOScale(1.2f, 0.35f).SetLoops(-1, LoopType.Yoyo).SetId(_timerText);
                _timerText.DOColor(_blinkColor, 0.35f).SetLoops(-1, LoopType.Yoyo).SetId(_timerText);
            }
        } else {
            if (_isCritical) {
                _isCritical = false;
                DOTween.Kill(_timerText);
                _timerText.transform.DOScale(1, 0.35f).SetId(_timerText);
                _timerText.DOColor(_startColor, 0.35f).SetId(_timerText);
            }
        }
    }
}
