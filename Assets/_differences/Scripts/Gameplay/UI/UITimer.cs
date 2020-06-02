using System;
using System.Collections;

using Airion.Extensions;

using TMPro;

using UnityEngine;

public class UITimer : MonoBehaviour {
    public event Action Expired;
    
    [SerializeField] TextMeshProUGUI _timerText = default;

    AsyncFunc<float> _timerRoutine;

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
        var timestamp = Time.time;

        while (Time.time - timestamp <= duration) {
            UpdateTimer(duration - (Time.time - timestamp));
            yield return null;
        }

        UpdateTimer(0);
        Expired?.Invoke();
    }

    void UpdateTimer(float time) {
        _timerText.text = time.ToString("F0");
    }
}
