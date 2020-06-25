using System;
using System.Collections;
using System.Collections.Generic;

using Doozy.Engine;
using Doozy.Engine.UI;

using UnityEngine;
using UnityEngine.SceneManagement;

using Zenject;

public class ConnectionHandler : MonoBehaviour {
    public event Action ConnectionError;
    //TODO: Transfer it to some game controller?
    public event Action GameReload;
    
    [SerializeField] UIView _errorView = default;
    
    [Inject] InternetReachabilityVerifier _reachabilityVerifier = default;
    [Inject] PlayFabLogin _playFabLogin = default;
    [Inject] PlayFabFacebookAuth _playFabFacebookAuth = default;

    void Start() {
        _reachabilityVerifier.statusChangedDelegate += OnStatusChangedDelegate;
        _playFabLogin.PlayFabLoginFailed += ShowErrorView;
        _playFabFacebookAuth.FacebookLoginFailed += ShowErrorView;
        CheckConnection(_reachabilityVerifier.status);
    }

    void OnDestroy() {
        _reachabilityVerifier.statusChangedDelegate -= OnStatusChangedDelegate;
        _playFabLogin.PlayFabLoginFailed -= ShowErrorView;
        _playFabFacebookAuth.FacebookLoginFailed -= ShowErrorView;
    }

    void OnStatusChangedDelegate(InternetReachabilityVerifier.Status newstatus) {
        CheckConnection(newstatus);
    }

    void CheckConnection(InternetReachabilityVerifier.Status status) {
        if (status == InternetReachabilityVerifier.Status.Error ||
            status == InternetReachabilityVerifier.Status.Offline ||
            status == InternetReachabilityVerifier.Status.Mismatch) {
            ConnectionError?.Invoke();
            _errorView.Show();
        }
    }

    [ContextMenu("DebugError")]
    public void ShowErrorView() {
        _errorView.Show();
    }

    [ContextMenu("Reload")]
    public void ReloadGame() {
        GameReload?.Invoke();
        _errorView.Hide();
        SceneManager.LoadScene(0);
        CheckConnection(_reachabilityVerifier.status);
    }
}