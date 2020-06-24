using System;

using PlayFab;
using PlayFab.ClientModels;

using UnityEngine;
using UnityEngine.Events;

using LoginResult = PlayFab.ClientModels.LoginResult;

public class PlayFabLogin : MonoBehaviour {
    public event Action PlayFabLogged = default;
    public event Action PlayFabLoginFailed = default;

    public bool IsLogged { get; private set; } = false;

    [SerializeField] bool _isLoginOnStart = default;

    void Awake() {
        if(_isLoginOnStart)
            LoginWithDeviceId();
    }

    public void LoginWithDeviceId() {
        #if UNITY_ANDROID
        var androidRequest = new LoginWithAndroidDeviceIDRequest
                             {AndroidDeviceId = SystemInfo.deviceUniqueIdentifier, CreateAccount = true};

        PlayFabClientAPI.LoginWithAndroidDeviceID(androidRequest, OnLoginSuccess, OnLoginFailure);
        #elif UNITY_IOS
        var IOSRequest = new LoginWithIOSDeviceIDRequest() {DeviceId = SystemInfo.deviceUniqueIdentifier ,CreateAccount
 = true};
        PlayFabClientAPI.LoginWithIOSDeviceID(IOSRequest, OnLoginSuccess, OnLoginFailure);
        #endif
    }

    void OnLoginSuccess(LoginResult result) {
        Debug.Log("PlayFab login success.");
        IsLogged = true;
        PlayFabLogged?.Invoke();
    }

    void OnLoginFailure(PlayFabError error) {
        Debug.LogError("PlayFab login error:");
        Debug.LogError(error.GenerateErrorReport());
        IsLogged = false;
        PlayFabLoginFailed?.Invoke();
    }
}