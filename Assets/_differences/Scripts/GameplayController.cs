using System;

using UnityEngine;
using UnityEngine.SceneManagement;

using Zenject;

public class GameplayController : MonoBehaviour {
    public event Action Began;
    public event Action<Data[]> Initialized;
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
        Load(1);
        Begin();
    }

    public void Load(int levelNum) {
        var levelData = _database.GetLevelByNum(levelNum);
        Initialized?.Invoke(new [] {levelData});
    }
    
    public void Begin() {
        Began?.Invoke();
    }

    public void StopLevel(bool result, PictureResult[] pictureResults) {
        Completed?.Invoke(new GameplayResult(result, pictureResults));
    }
}
