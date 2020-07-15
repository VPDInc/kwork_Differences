using System;

using TMPro;

using UnityEngine;

using Zenject;

public class UITimerUpdater : MonoBehaviour {
    [SerializeField] TextMeshProUGUI _timerText = default;

    [Inject] UITimer _timer = default;

    const string TIMER_FORMAT = "mm\\:ss";

    void Start() {
        _timer.TimerUpdated += OnTimerUpdated;
    }

    void OnDestroy() {
        _timer.TimerUpdated -= OnTimerUpdated;
    }

    void OnTimerUpdated(float time) {
        UpdateTimer(time);
    }

    void UpdateTimer(float time) {
        _timerText.text = TimeSpan.FromSeconds(time).ToString(TIMER_FORMAT);
    }
}
