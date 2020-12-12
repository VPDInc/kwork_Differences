using System;

using Airion.Currency;

using UnityEngine;
using UnityEngine.SignInWithApple;

using Zenject;

public class AppleLogin : MonoBehaviour {
    public event Action Initialized;
    public event Action Logged;
    
    public bool IsLogged { get; private set; } = false;
    public bool IsInitialized { get; private set; } = false;

    [SerializeField] int _linkReward = 200;

    [Inject] CurrencyManager _currencyManager = default;

    string UserId {
        get => PlayerPrefs.GetString("user-id", "");
        set => PlayerPrefs.SetString("user-id", value);
    }

    void Start() {
        LoadCredential();
    }

    public void Login() {
        if (!IsInitialized || IsLogged)
            return;

        var siwa = gameObject.GetComponent<SignInWithApple>();
        siwa.Login(OnLogin);
    }

    void LoadCredential() {
        // User id that was obtained from the user signed into your app for the first time.
        var siwa = gameObject.GetComponent<SignInWithApple>();
        siwa.GetCredentialState(UserId, OnCredentialState);
    }

    void OnCredentialState(SignInWithApple.CallbackArgs args) {
        Debug.Log(string.Format("User credential state is: {0}", args.credentialState));

        if (args.error != null) {
            Debug.Log(string.Format("Errors occurred: {0}", args.error));
        }

        IsInitialized = true;
        IsLogged = args.credentialState == UserCredentialState.Authorized;
        Initialized?.Invoke();
    }

    void OnLogin(SignInWithApple.CallbackArgs args) {
        if (args.error != null) {
            Debug.Log("Errors occurred: " + args.error);
            return;
        }

        UserInfo userInfo = args.userInfo;

        // Save the userId so we can use it later for other operations.
        UserId = userInfo.userId;

        // Print out information about the user who logged in.
        Debug.Log(string.Format("Display Name: {0}\nEmail: {1}\nUser ID: {2}\nID Token: {3}",
            userInfo.displayName ?? "",
            userInfo.email ?? "", userInfo.userId ?? "", userInfo.idToken ?? ""));

        IsLogged = true;
        Logged?.Invoke();
        Reward();
    }

    void Reward() {
        _currencyManager.GetCurrency(Differences.CurrencyConstants.SOFT).Earn(_linkReward);
    }
}
