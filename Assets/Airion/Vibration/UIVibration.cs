using UnityEngine;

using Zenject;

public class UIVibration : MonoBehaviour {
    [Inject] VibrationManager _vibrationManager = default;

    [SerializeField] GameObject _on = default;
    [SerializeField] GameObject _off = default;
    
    void Start() {
        _vibrationManager.MuteChanged += OnMuteChanged;
        SwitchVisual(_vibrationManager.IsOn);
    }

    void OnDestroy() {
        _vibrationManager.MuteChanged -= OnMuteChanged;
    }

    void OnMuteChanged() {
        SwitchVisual(_vibrationManager.IsOn);
    }

    void SwitchVisual(bool isOn) {
        if (_off != null)
            _off.SetActive(!isOn);
        if (_on != null) 
            _on.SetActive(isOn);
    }

    public void Switch() {
        _vibrationManager.Switch();
    }
    
}
