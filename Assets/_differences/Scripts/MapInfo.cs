using System;

using PathCreation;

using UnityEngine;

public class MapInfo : MonoBehaviour {
    [SerializeField] LevelInfo _levelPrefab = default;
    [SerializeField] PathCreator _pathCreator = default;
    [SerializeField] int _levelCount = 10;
    [SerializeField] Transform _levelHolder = default;

    void Start() {
        PopulateMap();
    }

    void PopulateMap() {
        var step = 1f / _levelCount;
        for (int i = 0; i < _levelCount; i++) {
            var level = Instantiate(_levelPrefab, _pathCreator.path.GetPointAtTime(step * i + step * 0.5f), Quaternion.identity, _levelHolder);
            level.Setup(i+1);
        }
    }

    void OnDrawGizmos() {
        var step = 1f / _levelCount;
        for (int i = 0; i < _levelCount; i++) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawIcon(_pathCreator.path.GetPointAtTime(step * i + step * 0.5f) + Vector3.up * 0.5f, "Misc/CupGizmo.png");
            // Gizmos.DrawSphere(_pathCreator.path.GetPointAtTime(step * i + step * 0.5f), 0.2f);
        }
    }
}