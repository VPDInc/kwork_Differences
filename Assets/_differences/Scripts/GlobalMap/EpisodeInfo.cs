using System;
using System.Collections.Generic;

using DG.Tweening;

using PathCreation;

using TMPro;

using UnityEngine;

using Zenject;

public class EpisodeInfo : MonoBehaviour {
    public static event Action EpisodeUnlocked;
    public int EpisodeNum => _episodeNum;
    public int LevelCount => _levelCount;
    public bool IsUnlocked => _isUnlocked;
    public List<LevelInfo> Levels => _levels;
    
    [Header("Episode Info")]
    [SerializeField] int _levelCount = 10;
    [SerializeField] bool _isUnlocked = false;
    [Header("Tech Info")] [SerializeField] PathCreator _pathCreator = default;
    [SerializeField] LevelInfo _levelPrefab = default;
    [SerializeField] Transform _levelHolder = default;
    [SerializeField] Transform _blockerRenderer = default;

    [Inject] DiContainer _diContainer = default;

    List<LevelInfo> _levels = new List<LevelInfo>();
    int _episodeNum = 0;


    public void Init(int levelOffset, int num) {
        _episodeNum = num;
        PopulateMap(levelOffset);
        if (_isUnlocked) {
            UnlockEpisode(true);
        }
    }

    public void UnlockEpisode(bool isInstant) {
        if(_isUnlocked) return;
        
        _isUnlocked = true;
        Fade(isInstant);
        // _blockerRenderer.DOFade(0, isInstant ? 0 : BLOCK_DISSOLVE_EFFECT_DURATION);
        if(isInstant) return;
        
        EpisodeUnlocked?.Invoke();
    }

    // void BlockEpisode(bool isInstant) {
    //     Fade(isInstant);
    //     // _blockerRenderer.DOFade(1, isInstant ? 0 : BLOCK_DISSOLVE_EFFECT_DURATION);
    // }

    void Fade(bool isInstant) {
        var clouds = _blockerRenderer.GetComponentsInChildren<CloudAnimation>();
        Array.ForEach(clouds, cloud => cloud.Hide(isInstant));
    }

    [ContextMenu("Unlock")]
    void DebugUnlock() {
        UnlockEpisode(false);
    }

    void PopulateMap(int levelOffset) {
        var step = 1f / _levelCount;
        for (int i = 0; i < _levelCount; i++) {
            var level = _diContainer
                        .InstantiatePrefab(_levelPrefab, _pathCreator.path.GetPointAtTime(step * i + step * 0.5f),
                                           Quaternion.identity, _levelHolder).GetComponent<LevelInfo>();

            level.Init(this, levelOffset + i);
            _levels.Add(level);
        }
    }

    void OnDrawGizmos() {
        var step = 1f / _levelCount;
        for (int i = 0; i < _levelCount; i++) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawIcon(_pathCreator.path.GetPointAtTime(step * i + step * 0.5f) + Vector3.up * 0.5f,
                            "Misc/CupGizmo.png");
        }
    }
}