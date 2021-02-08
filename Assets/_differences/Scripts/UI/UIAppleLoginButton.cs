using System;

using UnityEngine;

using Zenject;

public class UIAppleLoginButton : MonoBehaviour {

    [Inject] AppleLogin _appleLogin = default;
    [SerializeField] private CollectFx collectFx = default;
    [SerializeField] private bool enableFx = false;

    void Start() {
        if (Application.platform == RuntimePlatform.Android) {
            gameObject.SetActive(false);
            return;
        }

        _appleLogin.Initialized += OnInitialized;
        _appleLogin.Logged += OnLogged;
        
        if (_appleLogin.IsInitialized) {
            Setup();
        }
    }

    private void OnLogged()
    {
        if(enableFx)
        {
            collectFx.SetupTrailEffect(delegate { _appleLogin.SetReward(); } );
        }
        else
        {
            _appleLogin.SetReward();
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
