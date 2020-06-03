using System;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayController : MonoBehaviour {
    public event Action LevelStarted;
    
    [SerializeField] string _gameplaySceneName = "GameplayScene";

    void Start() {
        if (SceneManager.GetSceneByName(_gameplaySceneName).isLoaded)
            return;
        
        SceneManager.LoadScene(_gameplaySceneName, LoadSceneMode.Additive);
    }

    [ContextMenu("Load scene")]
    void DebugLoadLevel1() {
        StartLevel(1);
    }
    
    public void StartLevel(int levelNum) {
        LevelStarted?.Invoke();
    }

    public void StopLevel() {
        
    }
}
