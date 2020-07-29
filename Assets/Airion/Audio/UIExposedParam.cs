using UnityEngine;

using Zenject;

public class UIExposedParam : MonoBehaviour {
    [SerializeField] string _paramName = default;
    
    [Inject] ExposedAudioParams _exposedAudio = default;

    [SerializeField] GameObject _on = default;
    [SerializeField] GameObject _off = default;
    
    void Start() {
        _exposedAudio.Changed += OnMuteChanged;
        SwitchVisual(!_exposedAudio.IsMute(_paramName));
    }

    void OnDestroy() {
        _exposedAudio.Changed -= OnMuteChanged;
    }

    void OnMuteChanged(string paramName) {
        if (!paramName.Equals(_paramName))
            return;
        
        SwitchVisual(!_exposedAudio.IsMute(_paramName));
    }

    void SwitchVisual(bool isOn) {
        if (_off != null)
            _off.SetActive(!isOn);
        if (_on != null) 
            _on.SetActive(isOn);
    }

    public void Switch() {
        _exposedAudio.SwitchVolume(_paramName);
    }
}