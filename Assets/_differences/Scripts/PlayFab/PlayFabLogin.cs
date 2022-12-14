using System;

using PlayFab;
using PlayFab.ClientModels;

using UnityEngine;

using Zenject;

using LoginResult = PlayFab.ClientModels.LoginResult;

public class PlayFabLogin : MonoBehaviour {
    public event Action PlayFabLogged = default;
    public event Action PlayFabLoginFailed = default;
    public bool IsLogged { get; private set; } = false;
    public string PlayerPlayfabId { get; private set; } = string.Empty;

    [SerializeField] bool _isLoginOnStart = default;

    [Inject] ConnectionHandler _connectionHandler = default;

    void Awake() {
        if(_isLoginOnStart)
            LoginWithDeviceId();
    }

    void Start() {
        _connectionHandler.GameReload += Reload;

    }

    void OnDestroy() {
        _connectionHandler.GameReload -= Reload;
    }

    void LoginWithDeviceId() {
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

    void Reload() {
        IsLogged = false;
        if(_isLoginOnStart)
            LoginWithDeviceId();
    }

    void OnLoginSuccess(LoginResult result) {
        Debug.Log("PlayFab login success.");
        IsLogged = true;
        PlayerPlayfabId = result.PlayFabId;
        PlayFabLogged?.Invoke();
    }

    void OnLoginFailure(PlayFabError error) {
        Debug.LogError("PlayFab login error:");
        Debug.LogError(error.GenerateErrorReport());
        IsLogged = false;
        PlayFabLoginFailed?.Invoke();
    }
}