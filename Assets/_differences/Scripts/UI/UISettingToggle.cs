using System;

using Airion.Extensions;

using UnityEngine;
using UnityEngine.Events;

public class UISettingToggle : MonoBehaviour {
    [SerializeField] bool _defaultToggle = default;
    [SerializeField] string _settingID = default;
    [SerializeField] GameObject _onToggle = default, _offToggle = default;
    
    public ToggleEvent ToggleChanged;

    bool _toggle;

    void Awake() {
        _toggle = PrefsExtensions.GetBool(_settingID, _defaultToggle);
        SetToggle(_toggle);
    }

    public void SwitchToggle() {
        SetToggle(!_toggle);
    }

    void SetToggle(bool toggle) {
        _toggle = toggle;

        _onToggle.SetActive(toggle);
        _offToggle.SetActive(!toggle);

        PrefsExtensions.SetBool(_settingID, _toggle);

        ToggleChanged?.Invoke(toggle);
    }

    public bool Toggle => _toggle;
}

[System.Serializable]
public class ToggleEvent : UnityEvent<bool> { }