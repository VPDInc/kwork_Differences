using System.Collections.Generic;

using DG.Tweening;

using PathCreation;

using TMPro;

using UnityEngine;

using Zenject;

public class EpisodeInfo : MonoBehaviour {
    public int LevelCount => _levelCount;
    public bool IsUnlocked => _isUnlocked;
    public List<LevelInfo> Levels => _levels;
    
    [Header("Episode Info")] [SerializeField]
    string _episodeName = "Episode";
    [SerializeField] int _levelCount = 10;
    [SerializeField] bool _isUnlocked = false;
    [Header("Tech Info")] [SerializeField] PathCreator _pathCreator = default;
    [SerializeField] LevelInfo _levelPrefab = default;
    [SerializeField] Transform _levelHolder = default;
    [SerializeField] TMP_Text _episodeLabel = default;
    [SerializeField] SpriteRenderer _blockerRenderer = default;

    [Inject] DiContainer _diContainer = default;

    List<LevelInfo> _levels = new List<LevelInfo>();

    const float BLOCK_DISSOLVE_EFFECT_DURATION = 0.5f;

    public void Init(int levelOffset) {
        PopulateMap(levelOffset);
        if (_isUnlocked) {
            UnlockEpisode(true);
        }
    }

    public void UnlockEpisode(bool isInstant) {
        _blockerRenderer.DOFade(0, isInstant ? 0 : BLOCK_DISSOLVE_EFFECT_DURATION);
    }

    void BlockEpisode(bool isInstant) {
        _blockerRenderer.DOFade(1, isInstant ? 0 : BLOCK_DISSOLVE_EFFECT_DURATION);
    }

    void PopulateMap(int levelOffset) {
        _episodeLabel.text = _episodeName;
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