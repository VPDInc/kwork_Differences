using System;

using UnityEngine;
using UnityEngine.SceneManagement;

using Zenject;

public class GameplayController : MonoBehaviour {
    public event Action Began;
    public event Action<int, Data[]> Initialized;
    public event Action<GameplayResult> Completed;

    [SerializeField] string _gameplaySceneName = "GameplayScene";

    [Inject] Database _database = default;
    
    void Start() {
        if (SceneManager.GetSceneByName(_gameplaySceneName).isLoaded)
            return;
        
        SceneManager.LoadScene(_gameplaySceneName, LoadSceneMode.Additive);
    }

    [ContextMenu("Load scene")]
    void DebugLoadLevel1() {
        _database.Load(1);
        Load(1);
        Begin();
    }

    public void Load(int levelNum) {
        var data = _database.GetData(levelNum);
        Initialized?.Invoke(levelNum, data);
    }
    
    public void Begin() {
        Began?.Invoke();
    }

    public void StopLevel(bool result, PictureResult[] pictureResults) {
        Completed?.Invoke(new GameplayResult(result, pictureResults));
    }

#if UNITY_EDITOR
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space))
            DebugLoadLevel1();

        if (Input.GetKeyDown(KeyCode.F)) {
            if (_database.LoadSpecificJson(-1)) {
                var data = _database.GetData(-1);
                Initialized?.Invoke(-1, data);
                Begin();
            }
        }
    }
#endif

}
