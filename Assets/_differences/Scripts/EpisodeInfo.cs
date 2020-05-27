using System;

using Boo.Lang;

using DG.Tweening;

using PathCreation;

using TMPro;

using UnityEngine;

public class EpisodeInfo : MonoBehaviour {
    public int LevelCount => _levelCount;
    [Header("Episode Info")]
    [SerializeField] string _episodeName = "Episode";
    [SerializeField] int _levelCount = 10;
    [SerializeField] bool _isUnlocked = false;
    [Header("Tech Info")]
    [SerializeField] PathCreator _pathCreator = default;
    [SerializeField] LevelInfo _levelPrefab = default;
    [SerializeField] Transform _levelHolder = default;
    [SerializeField] TMP_Text _episodeLabel = default;
    [SerializeField] SpriteRenderer _blockerRenderer = default;

    List<LevelInfo> _levels = new List<LevelInfo>();
    float _blockDissolveEffectDuration = 0.5f;

    public void Init(int levelOffset) {
        PopulateMap(levelOffset);
        if (_isUnlocked) {
            UnlockEpisode(true);
        }
    }

    void UnlockEpisode(bool isInstant) {
        _blockerRenderer.DOFade(0, isInstant? 0 :_blockDissolveEffectDuration);
    }

    void BlockEpisode(bool isInstant) {
        _blockerRenderer.DOFade(1, isInstant? 0 :_blockDissolveEffectDuration);
    }

    void PopulateMap(int levelOffset) {
        _episodeLabel.text = _episodeName;
        var step = 1f / _levelCount;
        for (int i = 0; i < _levelCount; i++) {
            var level = Instantiate(_levelPrefab, _pathCreator.path.GetPointAtTime(step * i + step * 0.5f), Quaternion.identity, _levelHolder);
            level.Setup(levelOffset + i+1);
            _levels.Add(level);
        }
    }

    void OnDrawGizmos() {
        var step = 1f / _levelCount;
        for (int i = 0; i < _levelCount; i++) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawIcon(_pathCreator.path.GetPointAtTime(step * i + step * 0.5f) + Vector3.up * 0.5f, "Misc/CupGizmo.png");
        }
    }
}