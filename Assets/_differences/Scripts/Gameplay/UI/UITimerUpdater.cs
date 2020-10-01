using System;

using Airion.Audio;

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
    [Inject] AudioManager _audioManager = default;

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

        if (TimeSpan.FromSeconds(_lastTime).Seconds - TimeSpan.FromSeconds(time).Seconds == 1) {
            _timerText.text =  TimeSpan.FromSeconds(time).ToString(TIMER_FORMAT);
            if (_isCritical)
                _audioManager.PlayOnce("tick");
        } else {
            DOTween.Kill(this);
            var startTime = _lastTime;
            DOTween.To(() => startTime, x => {
                _timerText.text = TimeSpan.FromSeconds(x).ToString(TIMER_FORMAT);
            }, time, 0.8f).SetEase(Ease.Linear).SetId(this);
        }

        _lastTime = time;
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
