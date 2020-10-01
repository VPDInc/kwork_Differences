using System;

using DG.Tweening;

using UnityEngine;

public class VibrationManager : MonoBehaviour {
    public event Action MuteChanged;
    
    public bool IsOn {
        get => _isOn;
        set {
            if (value)
                Unmute();
            else 
                Mute();
        }
    }
    
    bool _isOn = false;

    const string IS_ON_STR = "IsVibrationOn";
    float _lastPeekTimestamp = 0;

    [SerializeField] float _peekCooldown = 0.1f;

    void Awake() {
        _isOn = PlayerPrefs.GetInt(IS_ON_STR, 1) == 1;
    }

    public void Switch() {
        IsOn = !IsOn;
    }

    public void Unmute() {
        PlayerPrefs.SetInt(IS_ON_STR, 1);
        _isOn = true;
        MuteChanged?.Invoke();
    }

    public void Mute() {
        PlayerPrefs.SetInt(IS_ON_STR, 0);
        _isOn = false;
        MuteChanged?.Invoke();
    }

    public void VibratePeekTimeout() {
        if (Time.time - _lastPeekTimestamp < _peekCooldown)
            return;
        
        VibratePeek();
        _lastPeekTimestamp = Time.time;
    }

    public void VibratePop() {
        if (!IsOn)
            return;
        
        Vibration.VibratePop();
    }
    
    public void VibratePeek() {
        if (!IsOn)
            return;
        
        Vibration.VibratePeek();
    }

    public void VibrateNope() {
        if (!IsOn)
            return;
        
        #if UNITY_IOS
            Vibration.VibrateNope();
        #else
            var seq = DOTween.Sequence();
            seq.AppendCallback(Vibration.VibratePeek);
            seq.AppendInterval(0.3f);
            seq.AppendCallback(Vibration.VibratePeek);
        #endif
    }
}
