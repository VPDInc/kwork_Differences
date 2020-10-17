using UnityEngine;

using System.Collections;

using Doozy.Engine;
using Doozy.Engine.Progress;
using Doozy.Engine.UI;

using Facebook.Unity;

using PlayFab.ClientModels;

using TMPro;

using UnityEngine.SceneManagement;

using Zenject;

public class LoadingScreenSystem : MonoBehaviour {
    [SerializeField] UIView _loadingScreen = default;
    [SerializeField] int _sceneToLoad = 1;
    [SerializeField] Progressor _bar = default;
    [SerializeField] TextMeshProUGUI _versionText = default;
    [SerializeField] string _versionPrefix = "v";
    [SerializeField] GameObject _facebookLoginButton = default;

    [Inject] PlayFabLogin _playFabLogin = default;
    [Inject] PlayFabInfo _playFabInfo = default;
    [Inject] PlayFabFacebook _playFabFacebook = default;
    [Inject] AppleLogin _appleLogin = default;

    AsyncOperation _async;

    const string GAME_LOADED_EVENT_NAME = "GameLoaded";

    void StartLoading(int sceneNo) {
        StartCoroutine(Loading(sceneNo));
    }

    void Awake() {
        Application.targetFrameRate = 60;
        _versionText.text = _versionPrefix + Application.version;
    }

    void Start() {
        _playFabInfo.AccountInfoRecieved += OnAccountInfoRecieved;
        _playFabFacebook.FacebookLogged += ProcessToGame;
        _playFabFacebook.FacebookLinked += ProcessToGame;
        _appleLogin.Logged += ProcessToGame;
        
        StartLoading(_sceneToLoad);
        _bar.SetProgress(0);
    }

    void OnAccountInfoRecieved(GetAccountInfoResult obj) {
        _facebookLoginButton.SetActive(!_playFabInfo.IsFacebookLinked);
        if(_playFabInfo.IsFacebookLinked && !FB.IsLoggedIn)
            _playFabFacebook.LoginFacebook();
    }

    void OnDestroy() {
        _playFabInfo.AccountInfoRecieved -= OnAccountInfoRecieved;
        _playFabFacebook.FacebookLogged -= ProcessToGame;
        _playFabFacebook.FacebookLinked -= ProcessToGame;
        _appleLogin.Logged -= ProcessToGame;
    }

    IEnumerator Loading(int sceneNo) {
        yield return new WaitForSeconds(0.5f);
        _async = SceneManager.LoadSceneAsync(sceneNo, LoadSceneMode.Additive);
        _async.allowSceneActivation = false;
        _bar.SetProgress(0);

        while (_async.progress < 0.9f) {
            var progress = Mathf.Clamp01(_async.progress / 1.5f);
            _bar.SetProgress(progress);
            
            yield return null;
        }

        bool IsReady() => _playFabLogin.IsLogged && _playFabFacebook.IsFacebookReady && _playFabInfo.IsAccountInfoUpdated && (_appleLogin.IsInitialized || Application.isEditor);

        while (!IsReady()) {
            _bar.SetProgress(Mathf.Lerp(_bar.Progress, 1, Time.deltaTime));
            yield return null;
        }
        
        _async.allowSceneActivation = true;
        _bar.SetProgress(1);
        GameEventMessage.SendEvent(GAME_LOADED_EVENT_NAME);
    }

    void DisableLoadingScreen() {
        _loadingScreen.Hide();
        SceneManager.UnloadSceneAsync("LoadingScene");
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(1));
    }

    public void ProcessToGame() {
        DisableLoadingScreen();
    }
}