using System;
using System.Collections;

using Airion.Extensions;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;

public class UITimer : MonoBehaviour {
    public event Action Expired;
    public event Action<float> TimerUpdated;
    
    [SerializeField] UIView _missClickView = default;
    [SerializeField] TextMeshProUGUI _timeReduceText = default;

    AsyncFunc<float> _timerRoutine;

    float _timeExpireTimestamp;
    float _timeLeft;

    const string TIME_REDUCE_TEXT = "-{0} sec";

    void Awake() {
        _timerRoutine = new AsyncFunc<float>(this, TimerRoutine);
    }

    public void Launch(float duration) {
        _timerRoutine.Start(duration);
    }

    public void Stop() {
        _timerRoutine.Stop();
    }
    
    void UpdateTimer(float time) {
        TimerUpdated?.Invoke(time);
    }

    public void ReduceTime(float reduceTime) {
        _timeReduceText.text = String.Format(TIME_REDUCE_TEXT, reduceTime);
        _missClickView.Show();
        _timeExpireTimestamp -= reduceTime;
    }

    public void AddTime(float timeBoost) {
        _timeExpireTimestamp += timeBoost;
    }

    public void Pause() {
        _timeLeft = _timeExpireTimestamp - Time.time;
        _timerRoutine.Stop();
    }

    public void Resume() {
        _timerRoutine.Start(_timeLeft);
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

}
