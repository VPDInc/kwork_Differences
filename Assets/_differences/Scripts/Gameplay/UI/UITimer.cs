using System;
using System.Collections;

using Airion.Extensions;

using TMPro;

using UnityEngine;

public class UITimer : MonoBehaviour {
    public event Action Expired;
    
    [SerializeField] TextMeshProUGUI _timerText = default;
    [SerializeField] UIMissClickWarning _uiMissClickWarningPrefab = default;

    AsyncFunc<float> _timerRoutine;

    float _timeExpireTimestamp;

    void Awake() {
        _timerRoutine = new AsyncFunc<float>(this, TimerRoutine);
    }

    public void Launch(float duration) {
        _timerRoutine.Start(duration);
    }

    public void Stop() {
        _timerRoutine.Stop();
    }

    IEnumerator TimerRoutine(float duration) {
        UpdateTimer(duration);
        _timeExpireTimestamp = Time.time + duration;

        while (_timeExpireTimestamp - Time.time >= 0) {
            var time = Mathf.Max(0, _timeExpireTimestamp - Time.time);
            UpdateTimer(time);
            yield return null;
        }

        UpdateTimer(0);
        Expired?.Invoke();
    }

    void UpdateTimer(float time) {
        _timerText.text = time.ToString("F0");
    }

    public void ReduceTime(float reduceTime) {
        var missClick = Instantiate(_uiMissClickWarningPrefab, transform);
        missClick.SetReducedTimeAndRun(reduceTime);
        
        _timeExpireTimestamp -= reduceTime;
    }

    public void AddTime(float timeBoost) {
        _timeExpireTimestamp += timeBoost;
    }
}
