using PlayFab;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using PlayFab.PfEditor;

namespace _differences.Scripts.Editor
{
    [CustomEditor(typeof(Configs.GameConfigWrapper))]
    public class GameConfigWrapperInspector : UnityEditor.Editor
    {
        private const string PlayFabUploadTargetCacheFileName = "PlayFabUploadTargetCache.json";

        private const string FILE_SERVER_NAME = "config.json";

        private struct PlayFabUploadTarget
        {
            public string titleId;
            public string devKey;

            public static bool operator ==(PlayFabUploadTarget a, PlayFabUploadTarget b)
            {
                return a.devKey == b.devKey && a.titleId == b.titleId;
            }

            public static bool operator !=(PlayFabUploadTarget a, PlayFabUploadTarget b)
            {
                return (a == b) == false;
            }
        }

        private string configFilePath;
        private PlayFabUploadTarget playFabUploadTarget;

        private SerializedProperty gameConfig;

        private void OnEnable()
        {
            gameConfig = serializedObject.FindProperty(nameof(Configs.GameConfigWrapper.gameConfig));

            string pathToCache = Path.Combine(Application.persistentDataPath, PlayFabUploadTargetCacheFileName);

            if (File.Exists(pathToCache) == true)
            {
                try
                {
                    playFabUploadTarget = JsonUtility.FromJson<PlayFabUploadTarget>(File.ReadAllText(pathToCache));
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button($"Load...") == true)
            {
                configFilePath = EditorUtility.OpenFilePanel("Open config file", Application.dataPath, "json");
                if (File.Exists(configFilePath) == true)
                {
                    string fileBody = File.ReadAllText(configFilePath);
                    (target as Configs.GameConfigWrapper).SetGameConfigFromJson(fileBody);
                    EditorUtility.SetDirty(target);
                }
                else
                {
                    configFilePath = null;
                }
            }

            if (GUILayout.Button("Save to..."))
            {
                string filePath = EditorUtility.SaveFilePanel("Save config file", Application.dataPath, "file.json", "json");
                SaveToFile(filePath);
            }

            if (string.IsNullOrEmpty(configFilePath) == false)
            {
                if (GUILayout.Button($"Save to '{configFilePath}'") == true)
                {
                    SaveToFile(configFilePath);
                }
            }

            playFabUploadTarget.titleId = PlayFabSettings.staticSettings.TitleId;
            playFabUploadTarget.devKey = PlayFabSettings.staticSettings.DeveloperSecretKey;

            GUILayout.Label($"Title Id: {PlayFabSettings.staticSettings.TitleId}");
            GUILayout.Label($"Dev Key: {PlayFabSettings.staticSettings.DeveloperSecretKey}");

            if (GUILayout.Button("Upload to PlayFab"))
            {
                UploadToPlayFab();
            }

            serializedObject.Update();
            EditorGUILayout.PropertyField(gameConfig);
            serializedObject.ApplyModifiedProperties();
        }

        private void SaveToFile(string filePath)
        {
            File.WriteAllText(filePath, (target as Configs.GameConfigWrapper).GetGameConfigAsJson(true));
        }

        private void UploadToPlayFab()
        {
            string body = (target as Configs.GameConfigWrapper).GetGameConfigAsJson(false);

            Upload(playFabUploadTarget.titleId, playFabUploadTarget.devKey, body,
                () => { Debug.Log("<color=green>Success!</color>"); },
                ex => { Debug.LogError(ex); });
        }

        private void Upload(string titleId, string secretKey, string body, Action onSuccses, Action<string> onFailed)
        {
            var uploadBody = new Dictionary<string, string>()
            {
                {"Config", body }
            };

            PlayFabEditorApi.SetTitleData(uploadBody,
                uploadResponse =>
                   onSuccses?.Invoke()
                ,
                uploadError =>
                    onFailed?.Invoke(uploadError.ErrorMessage)
            );
        }
    }
}