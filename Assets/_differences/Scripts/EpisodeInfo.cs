using System;

using PathCreation;

using TMPro;

using UnityEngine;

public class EpisodeInfo : MonoBehaviour {
    public int LevelCount => _levelCount;
    
    [SerializeField] string _episodeName = "Episode";
    [SerializeField] LevelInfo _levelPrefab = default;
    [SerializeField] PathCreator _pathCreator = default;
    [SerializeField] int _levelCount = 10;
    [SerializeField] Transform _levelHolder = default;
    [SerializeField] TMP_Text _episodeLabel = default;

    public void Init(int levelOffset) {
        PopulateMap(levelOffset);
    }
    
    void PopulateMap(int levelOffset) {
        _episodeLabel.text = _episodeName;
        var step = 1f / _levelCount;
        for (int i = 0; i < _levelCount; i++) {
            var level = Instantiate(_levelPrefab, _pathCreator.path.GetPointAtTime(step * i + step * 0.5f), Quaternion.identity, _levelHolder);
            level.Setup(levelOffset + i+1);
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