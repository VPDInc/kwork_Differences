using System;
using System.Collections.Generic;
using PathCreation;
using UnityEditor;
using UnityEngine;
using Zenject;

public class EpisodeInfo : MonoBehaviour
{
    public enum LevelСreationMode
    {
        PatchCreator,
        Manual
    }

    public static event Action EpisodeUnlocked;

    [SerializeField] private LevelСreationMode _levelСreationMode;
    [SerializeField] [Min(0)] private int _levelCount = 10;
    [SerializeField] private LevelInfo[] _levels;
    [SerializeField] private bool _isUnlocked = false;

    [SerializeField] [Min(0)] private float _stepBetweenLevels = 1f;
    [SerializeField] private PathCreator _pathCreator = default;
    [SerializeField] private LevelInfo _levelPrefab = default;
    [SerializeField] private Transform _levelHolder = default;
    [SerializeField] private Transform _blockerRenderer = default;

    [Inject] DiContainer _diContainer = default;

    private List<LevelInfo> _levelsList = new List<LevelInfo>();
    private int _episodeNum = 0;

    public int EpisodeNum => _episodeNum;
    public int LevelCount => _levelsList.Count;
    public bool IsUnlocked => _isUnlocked;
    public List<LevelInfo> Levels => _levelsList;

    public void Init(int levelOffset, int num) {
        _episodeNum = num;
        PopulateMap(levelOffset);
        if (_isUnlocked) {
            UnlockEpisode(true);
        }
    }

    public void UnlockEpisode(bool isInstant)
    {
        if (_isUnlocked) return;
        
        _isUnlocked = true;
        Fade(isInstant);
        // _blockerRenderer.DOFade(0, isInstant ? 0 : BLOCK_DISSOLVE_EFFECT_DURATION);

        if (isInstant) return;
        EpisodeUnlocked?.Invoke();
    }

    // void BlockEpisode(bool isInstant) {
    //     Fade(isInstant);
    //     // _blockerRenderer.DOFade(1, isInstant ? 0 : BLOCK_DISSOLVE_EFFECT_DURATION);
    // }

    private void Fade(bool isInstant)
    {
        var clouds = _blockerRenderer.GetComponentsInChildren<CloudAnimation>();
        Array.ForEach(clouds, cloud => cloud.Hide(isInstant));
    }

    [ContextMenu("Unlock")]
    private void DebugUnlock()
    {
        UnlockEpisode(false);
    }

    private void PopulateMap(int levelOffset)
    {
        var index = 0;
        var step = _stepBetweenLevels / _levelCount;

        if (_levelСreationMode == LevelСreationMode.PatchCreator)
        {
            for (index = 0; index < _levelCount; index++)
            {
                var level = _diContainer.InstantiatePrefab(_levelPrefab,
                    _pathCreator.path.GetPointAtTime(step * index + step * 0.5f),
                    Quaternion.identity, _levelHolder).GetComponent<LevelInfo>();

                level.Init(this, levelOffset + index);
                _levelsList.Add(level);
            }
        }

        foreach (LevelInfo level in _levels)
        {
            level.Init(this, levelOffset + index);
            index++;
        }

        _levelsList.AddRange(_levels);
    }

    private void OnDrawGizmos()
    {
        var step = _stepBetweenLevels / _levelCount;

        for (int i = 0; i < _levelCount; i++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawIcon(_pathCreator.path.GetPointAtTime(step * i + step * 0.5f) +
                Vector3.up * 0.5f, "Misc/CupGizmo.png");
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EpisodeInfo))]
public class EpisodeInfoEditor : Editor
{
    private SerializedProperty _levelСreationMode;
    private SerializedProperty _levelCount;
    private SerializedProperty _levels;
    private SerializedProperty _isUnlocked;

    private SerializedProperty _stepBetweenLevels;
    private SerializedProperty _pathCreator;
    private SerializedProperty _levelPrefab;
    private SerializedProperty _levelHolder;
    private SerializedProperty _blockerRenderer;

    private void OnEnable()
    {
        _levelСreationMode = serializedObject.FindProperty("_levelСreationMode");
        _levelCount = serializedObject.FindProperty("_levelCount");
        _levels = serializedObject.FindProperty("_levels");
        _isUnlocked = serializedObject.FindProperty("_isUnlocked");

        _stepBetweenLevels = serializedObject.FindProperty("_stepBetweenLevels");
        _pathCreator = serializedObject.FindProperty("_pathCreator");
        _levelPrefab = serializedObject.FindProperty("_levelPrefab");
        _levelHolder = serializedObject.FindProperty("_levelHolder");
        _blockerRenderer = serializedObject.FindProperty("_blockerRenderer");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.LabelField("Episode Info", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_levelСreationMode, new GUIContent("Level Сreation Mode"));

        if (_levelСreationMode.enumValueIndex == 0)
        {
            EditorGUILayout.PropertyField(_levelCount, new GUIContent("Level count"));
            EditorGUILayout.PropertyField(_levels, new GUIContent("Additional Levels"));
        }
        else
        {
            EditorGUILayout.PropertyField(_levels, new GUIContent("Levels"));
        }
        EditorGUILayout.PropertyField(_isUnlocked, new GUIContent("Is Unlocked"));

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Episode Tech", EditorStyles.boldLabel);

        if (_levelСreationMode.enumValueIndex == 0)
        {
            EditorGUILayout.PropertyField(_stepBetweenLevels, new GUIContent("Step Between Levels"));
            EditorGUILayout.PropertyField(_pathCreator, new GUIContent("Path Creator"));
        }

        EditorGUILayout.PropertyField(_levelPrefab, new GUIContent("Level Prefab"));
        EditorGUILayout.PropertyField(_levelHolder, new GUIContent("Level Holder"));
        EditorGUILayout.PropertyField(_blockerRenderer, new GUIContent("Blocker Renderer"));
        serializedObject.ApplyModifiedProperties();
    }
}
#endif