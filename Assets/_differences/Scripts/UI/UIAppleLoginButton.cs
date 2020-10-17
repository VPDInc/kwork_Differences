using System;

using UnityEngine;

using Zenject;

public class UIAppleLoginButton : MonoBehaviour {
    [Inject] AppleLogin _appleLogin = default;

    void Start() {
        if (Application.platform == RuntimePlatform.Android) {
            gameObject.SetActive(false);
            return;
        }

        _appleLogin.Initialized += OnInitialized;
        
        if (_appleLogin.IsInitialized) {
            Setup();
        }
    }

    void OnDestroy() {
        if (Application.platform == RuntimePlatform.Android)
            return;
        
        _appleLogin.Initialized -= OnInitialized;
    }

    public void OnLoginClick() {
        _appleLogin.Login();
    }

    void OnInitialized() {
        Setup();
    }

    void Setup() {
        gameObject.SetActive(!_appleLogin.IsLogged);
    }
}
