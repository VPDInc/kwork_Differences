using System;

using PlayFab;
using PlayFab.ClientModels;

using UnityEngine;

using Zenject;

using LoginResult = PlayFab.ClientModels.LoginResult;

public class PlayFabLogin : MonoBehaviour {
    public event Action PlayFabLogged = default;
    public event Action PlayFabLoginFailed = default;
    public event Action<GetAccountInfoResult> AccountInfoRecieved = default;

    public bool IsLogged { get; private set; } = false;
    public bool IsFacebookLinked { get; private set; } = false;
    public bool IsRecievedAccountInfo { get; private set; } = false;
    public GetAccountInfoResult AccountInfo { get; private set; } = default;

    [SerializeField] bool _isLoginOnStart = default;

    [Inject] ConnectionHandler _connectionHandler = default;
    [Inject] PlayFabFacebook _playFabFacebook = default;

    void Awake() {
        if(_isLoginOnStart)
            LoginWithDeviceId();
    }

    void Start() {
        _connectionHandler.GameReload += Reload;
        _playFabFacebook.FacebookLinked += OnFacebookLinked;
        _playFabFacebook.FacebookUnlinked += OnFacebookUnlinked;
    }

    void OnFacebookUnlinked() {
        IsFacebookLinked = false;
    }

    void OnFacebookLinked() {
        IsFacebookLinked = true;
    }

    void OnDestroy() {
        _connectionHandler.GameReload -= Reload;
        _playFabFacebook.FacebookLinked -= OnFacebookLinked;
        _playFabFacebook.FacebookUnlinked -= OnFacebookUnlinked;
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
        PlayFabLogged?.Invoke();
        GetAccountInfo();
    }

    void OnLoginFailure(PlayFabError error) {
        Debug.LogError("PlayFab login error:");
        Debug.LogError(error.GenerateErrorReport());
        IsLogged = false;
        PlayFabLoginFailed?.Invoke();
    }
    
    void GetAccountInfo() {
        var accountInfoRequest = new GetAccountInfoRequest();
        PlayFabClientAPI.GetAccountInfo(accountInfoRequest, AccountRequestSuccess, AccountRequestError);
    }

    void AccountRequestError(PlayFabError obj) {
        Debug.LogError(obj.GenerateErrorReport());
    }

    void AccountRequestSuccess(GetAccountInfoResult obj) {
        AccountInfo = obj;
        IsFacebookLinked = obj.AccountInfo.FacebookInfo != null;
        AccountInfoRecieved?.Invoke(AccountInfo);
    }
}