using System;
using System.Collections;
using System.Collections.Generic;

using PlayFab;
using PlayFab.ClientModels;

using UnityEngine;

using Zenject;

public class PlayFabInfo : MonoBehaviour {
    public event Action<GetAccountInfoResult> AccountInfoRecieved = default;
    
    public bool IsFacebookLinked { get; private set; } = false;
    public bool IsAccountInfoUpdated { get; private set; } = false;
    public GetAccountInfoResult AccountInfo { get; private set; } = default;
    
    [Inject] PlayFabLogin _playFabLogin = default;
    [Inject] PlayFabFacebook _playFabFacebook = default;

    void Start() {
        _playFabLogin.PlayFabLogged += GetAccountInfo;
        _playFabFacebook.FacebookLinked += OnFacebookLinked;
        _playFabFacebook.FacebookUnlinked += OnFacebookUnlinked;
    }

    void OnDestroy() {
        _playFabLogin.PlayFabLogged -= GetAccountInfo;
        _playFabFacebook.FacebookLinked -= OnFacebookLinked;
        _playFabFacebook.FacebookUnlinked -= OnFacebookUnlinked;
    }

    #region SetName

    public void SetPlayFabName(string name) {
        var updateUserTitleDisplayNameRequest = new UpdateUserTitleDisplayNameRequest {DisplayName = name};
        PlayFabClientAPI.UpdateUserTitleDisplayName(updateUserTitleDisplayNameRequest, UpdateNameResult,
                                                    UpdateNameError);
    }

    void UpdateNameError(PlayFabError obj) {
        Debug.LogError(obj.GenerateErrorReport());
    }

    void UpdateNameResult(UpdateUserTitleDisplayNameResult obj) {
        IsAccountInfoUpdated = false;
        GetAccountInfo();
        Debug.Log("Name updated to " + obj.DisplayName);
    }

    #endregion

    #region GetName

    public string GetName() {
        return AccountInfo.AccountInfo.Username;
    }

    #endregion

    #region GetAccountInfo
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
        IsAccountInfoUpdated = true;
        AccountInfoRecieved?.Invoke(AccountInfo);
    }

    #endregion

    #region Facebook

    void OnFacebookUnlinked() {
        IsFacebookLinked = false;
    }

    void OnFacebookLinked() {
        IsFacebookLinked = true;
    }

    #endregion
}