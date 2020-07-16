using System;

using TMPro;

using UnityEngine;

using Zenject;

public class UITimerUpdater : MonoBehaviour {
    [SerializeField] TextMeshProUGUI _timerText = default;
    [SerializeField] SimpleRotator _rotator = default;

    [Inject] UITimer _timer = default;

    const string TIMER_FORMAT = "mm\\:ss";

    void Start() {
        _timer.TimerUpdated += OnTimerUpdated;
    }

    void OnDestroy() {
        _timer.TimerUpdated -= OnTimerUpdated;
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
        _timerText.text = TimeSpan.FromSeconds(time).ToString(TIMER_FORMAT);
    }
}
