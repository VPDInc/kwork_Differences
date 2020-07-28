using System;

using UnityEngine;
using UnityEngine.Audio;

public class ExposedAudioParams : MonoBehaviour {
    public event Action<string> Changed;
    
    [SerializeField] AudioMixer _mixer = default;
    [SerializeField] string[] _volumeParams = default;

    const string PARAM_PREFIX = "vol_";
    const float LOW_VOLUME = -80;
    
    void Start() {
        Load();
    }

    public void SwitchVolume(string param) {
        if (_mixer.GetFloat(param, out var value)) {
            if (Mathf.Approximately(value, LOW_VOLUME))
                SetParamVolume(param, 0); 
            else
                SetParamVolume(param, LOW_VOLUME); 
        }
        
        Save();
    }

    public bool IsMute(string param) {
        if (_mixer.GetFloat(param, out var value)) 
            return Mathf.Approximately(value, LOW_VOLUME);

        return false;
    }
    
    void Load() {
        foreach (var volumeParam in _volumeParams) {
            var val = PlayerPrefs.GetFloat(PARAM_PREFIX + volumeParam, 0);
            SetParamVolume(volumeParam, val);
        }
    }

    void Save() {
        foreach (var param in _volumeParams) {
            if (_mixer.GetFloat(param, out var value))
                PlayerPrefs.SetFloat(PARAM_PREFIX + param, value);
        }
    }

    void SetParamVolume(string paramName, float volume) {
        _mixer.SetFloat(paramName, volume);
        Changed?.Invoke(paramName);
    }
}
