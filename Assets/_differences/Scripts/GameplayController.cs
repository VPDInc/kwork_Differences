using System;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayController : MonoBehaviour {
    public event Action Began;
    public event Action<GameplayResult> Completed;

    [SerializeField] string _gameplaySceneName = "GameplayScene";

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
        //TODO: Implement loading pictures logic
    }
    
    public void Begin() {
        Began?.Invoke();
    }

    public void StopLevel() {
        //DUMMY
        Completed?.Invoke(new GameplayResult());
    }
}
