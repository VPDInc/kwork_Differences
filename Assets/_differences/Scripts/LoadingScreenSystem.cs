using UnityEngine;

using System.Collections;

using Doozy.Engine;
using Doozy.Engine.Progress;
using Doozy.Engine.UI;

using TMPro;

using UnityEngine.SceneManagement;

using Zenject;

public class LoadingScreenSystem : MonoBehaviour {
    [SerializeField] UIView _loadingScreen = default;
    [SerializeField] int _sceneToLoad = 1;
    [SerializeField] Progressor _bar = default;
    [SerializeField] TextMeshProUGUI _versionText = default;
    [SerializeField] string _versionPrefix = "v";

    [Inject] PlayFabLogin _playFabLogin = default;
    [Inject] PlayFabFacebookAuth _playFabFacebookAuth = default;

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
        StartLoading(_sceneToLoad);
        _bar.SetProgress(0);
    }

    void OnDestroy() {
        _async.completed -= AsyncOnCompleted;
    }

    IEnumerator Loading(int sceneNo) {
        yield return new WaitForSeconds(0.5f);
        _async = SceneManager.LoadSceneAsync(sceneNo, LoadSceneMode.Additive);
        _async.completed += AsyncOnCompleted;
        _async.allowSceneActivation = false;
        _bar.SetProgress(0);

        while (_async.isDone == false) {
            var progress = Mathf.Clamp01(_async.progress / 1.5f);
            _bar.SetProgress(progress);
            
            if (_async.progress == 0.9f) {
                _async.allowSceneActivation = true;
            }
            
            yield return null;
        }

        while (!_playFabLogin.IsLogged && !_playFabFacebookAuth.IsFacebookReady) {
            _bar.SetProgress(Mathf.Lerp(_bar.Progress, 1, Time.deltaTime));
            yield return null;
        }

        _bar.SetProgress(1);
    }

    void AsyncOnCompleted(AsyncOperation obj) {
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