using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Difference
{
    public class LoadingSystem : MonoBehaviour
    {
        public event Action<Scene> SceneChanged;

        private void Start()
        {
            SceneManager.sceneLoaded += SceneManagerSceneLoaded;
            SceneManager.activeSceneChanged += SceneManageActiveSceneChanged;
        }

        public void SetActiveScene(Scene scene) => SceneManager.SetActiveScene(scene);
        public AsyncOperation LoadAsyncScene(int sceneId) => SceneManager.LoadSceneAsync(sceneId);

        private void SceneManageActiveSceneChanged(Scene arg0, Scene arg1)
        {
            Debug.Log($"Scene <color=green>{arg0.name}</color> was changes on  <color=green>{arg1.name}</color>");

            SceneChanged?.Invoke(arg1);
        }

        private void SceneManagerSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            Debug.Log($"Scene  <color=red>{arg0.name}</color> was loaded");
        }
    }
}