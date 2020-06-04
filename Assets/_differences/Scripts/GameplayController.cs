using System;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayController : MonoBehaviour {
    public event Action LevelStarted;
    public event Action<LevelResultInfo> LevelCompleted;

    
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

    public void LoadPicture() {
        //TODO: Implement loading picture logic
        Debug.Log("Pretend we're loading a new picture");
    }
    
    public void StartLevel(int levelNum) {
        LevelStarted?.Invoke();
    }

    public void StopLevel() {
        //DUMMY
        LevelCompleted?.Invoke(new LevelResultInfo());
    }
}
