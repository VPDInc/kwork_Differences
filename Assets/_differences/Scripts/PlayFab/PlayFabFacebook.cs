// Import statements introduce all the necessary classes for this example.

using System;
using System.Collections.Generic;

using Airion.Currency;
using Airion.Extensions;

using Doozy.Engine;

using Facebook.Unity;

using PlayFab;
using PlayFab.ClientModels;

using UnityEngine;
using UnityEngine.Events;

using Zenject;

using Currency = Airion.Currency.Currency;
using LoginResult = PlayFab.ClientModels.LoginResult;

public class PlayFabFacebook : MonoBehaviour {
    public event Action FacebookReady = default;
    public event Action FacebookLogged = default;
    public event Action FacebookLinked = default;
    public event Action FacebookUnlinked = default;
    public event Action FacebookError = default;

    public bool IsFacebookReady => FB.IsInitialized;
    public bool IsFacebookLogged => FB.IsLoggedIn;
    public bool WasLinked => _wasLinked;

    [SerializeField] int _facebookLinkReward = 200;
    [SerializeField] string _currencyName = "Soft";

    [SerializeField] bool _isUsingAnnoyingDebugText = false;

    [Inject] ConnectionHandler _connectionHandler = default;
    [Inject] CurrencyManager _currencyManager = default;

    Currency _currency = default;

    string _message;
    bool _wasLinked;

    const string CONNECTION_ATTEMPT_EVENT_NAME = "ConnectionAttempt";
    const string WAS_LINKED_ID = "was_linked";

    void Start() {
        InitFB();
        _connectionHandler.GameReload += Reload;

        _currency = _currencyManager.GetCurrency(_currencyName);

        _wasLinked = PrefsExtensions.GetBool(WAS_LINKED_ID, false);
    }

    public void LoginFacebook() {
        GameEventMessage.SendEvent(CONNECTION_ATTEMPT_EVENT_NAME);
        SetMessage("Logging into Facebook...");

        // We invoke basic login procedure and pass in the callback to process the result
        FB.LogInWithReadPermissions(new List<string>() {"public_profile", "email", "user_friends"}, OnFacebookLoggedIn);
    }

    public void LinkFacebook() {
        if (!FB.IsLoggedIn) {
            LoginFacebook();
            return;
        }

        SetMessage("Link Facebook to PlayFab account");
        var linkRequest = new LinkFacebookAccountRequest
                          {AccessToken = AccessToken.CurrentAccessToken.TokenString, ForceLink = true};

        PlayFabClientAPI.LinkFacebookAccount(linkRequest, OnFacebookLinkComplete, OnFacebookLinkError);
    }

    public void UnlinkFacebook() {
        SetMessage("Unlinking Facebook from PlayFab account");

        var unlinkRequest = new UnlinkFacebookAccountRequest();

        PlayFabClientAPI.UnlinkFacebookAccount(unlinkRequest, OnFacebookUnlinkComplete, OnFacebookUnlinkError);
    }

    void OnFacebookUnlinkError(PlayFabError obj) {
        SetMessage("Unlink Facebook from PlayFab account error: " + obj.GenerateErrorReport(), true);

        FacebookError?.Invoke();
    }

    void OnFacebookUnlinkComplete(UnlinkFacebookAccountResult obj) {
        SetMessage("Unlinked Facebook from PlayFab account");
        FB.LogOut();
        FacebookUnlinked?.Invoke();
    }

    public void Reload() {
        InitFB();
    }

    void InitFB() {
        if (FB.IsInitialized) return;

        SetMessage("Initializing Facebook..."); // logs the given message and displays it on the screen using OnGUI method

        // This call is required before any other calls to the Facebook API. We pass in the callback to be invoked once initialization is finished
        FB.Init(OnFacebookInitialized);
    }

    void OnFacebookLinkError(PlayFabError error) {
        SetMessage("PlayFab Facebook Link Failed: " + error.GenerateErrorReport(), true);

        FacebookError?.Invoke();
    }

    void OnFacebookLinkComplete(LinkFacebookAccountResult obj) {
        SetMessage("Facebook linked.");

        if (!_wasLinked) {
            _wasLinked = true;
            PrefsExtensions.SetBool(WAS_LINKED_ID, _wasLinked);
            _currency.Earn(_facebookLinkReward);
        }

        FacebookLinked?.Invoke();
    }

    void OnFacebookInitialized() {
        SetMessage("Facebook initialized.");
        FacebookReady?.Invoke();
    }

    void OnFacebookLoggedIn(ILoginResult result) {
        // If result has no errors, it means we have authenticated in Facebook successfully
        if (result == null || string.IsNullOrEmpty(result.Error)) {
            SetMessage("Facebook Auth Complete! Access Token: " + AccessToken.CurrentAccessToken.TokenString +
                       "\nLogging into PlayFab...");

            /*
             * We proceed with making a call to PlayFab API. We pass in current Facebook AccessToken and let it create
             * and account using CreateAccount flag set to true. We also pass the callback for Success and Failure results
             */
            LinkFacebook();
            // PlayFabClientAPI
            //     .LoginWithFacebook(new LoginWithFacebookRequest {CreateAccount = true, AccessToken = AccessToken.CurrentAccessToken.TokenString},
            //                        OnPlayFabFacebookAuthComplete, OnPlayFabFacebookAuthFailed);
        } else {
            // If Facebook authentication failed, we stop the cycle with the message
            SetMessage("Facebook Auth Failed: " + result.Error + "\n" + result.RawResult, true);
            FacebookError?.Invoke();
        }
    }

    // When processing both results, we just set the message, explaining what's going on.
    void OnPlayFabFacebookAuthComplete(LoginResult result) {
        SetMessage("PlayFab Facebook Auth Complete. Session ticket: " + result.SessionTicket);

        FacebookLogged?.Invoke();
    }

    void OnPlayFabFacebookAuthFailed(PlayFabError error) {
        SetMessage("PlayFab Facebook Auth Failed: " + error.GenerateErrorReport(), true);

        FacebookError?.Invoke();
    }

    void SetMessage(string message, bool error = false) {
        _message = message;
        if (error)
            Debug.LogError(message);
        else
            Debug.Log(message);
    }

    public void OnGUI() {
        if (!_isUsingAnnoyingDebugText) return;

        var style = new GUIStyle {
                                     fontSize = 40, normal = new GUIStyleState {textColor = Color.white},
                                     alignment = TextAnchor.MiddleCenter, wordWrap = true
                                 };

        var area = new Rect(0, 0, Screen.width, Screen.height);
        GUI.Label(area, _message, style);
    }
}