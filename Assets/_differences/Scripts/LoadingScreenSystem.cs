using System;

using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using Doozy.Engine.Progress;
using Doozy.Engine.UI;

using TMPro;

using UnityEngine.SceneManagement;

public class LoadingScreenSystem : MonoBehaviour {
    [SerializeField] UIView _loadingScreen = default;
    [SerializeField] int _sceneToLoad = 1;
    [SerializeField] Progressor _bar = default;
    [SerializeField] TextMeshProUGUI _versionText = default;
    [SerializeField] string _versionPrefix = "v";

    AsyncOperation _async;
    // bool _isFirstLevelLoaded;

    void StartLoading(int sceneNo) {
        StartCoroutine(Loading(sceneNo));
    }

    void Awake() {
        _versionText.text = _versionPrefix + Application.version;
    }

    void Start() {
        // LevelController.FirstLevelLoaded += OnFirstLevelLoaded;
        StartLoading(_sceneToLoad);
        _bar.SetProgress(0);
    }

    void OnDestroy() {
        // LevelController.FirstLevelLoaded -= OnFirstLevelLoaded;
        _async.completed -= AsyncOnCompleted;
    }

    void OnFirstLevelLoaded() {
        // _isFirstLevelLoaded = true;
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

        // while (!_isFirstLevelLoaded) {
        //     _bar.SetProgress(Mathf.Lerp(_bar.Progress, 1, Time.deltaTime));
        //     yield return null;
        // }

        _bar.SetProgress(1);
        DisableLoadingScreen();
    }

    void AsyncOnCompleted(AsyncOperation obj) {
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(1));
    }

    void DisableLoadingScreen() {
        _loadingScreen.Hide();
        SceneManager.UnloadSceneAsync("LoadingScene");
    }
}