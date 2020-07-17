using System;
using System.Collections.Generic;

using Airion.Extensions;

using UnityEngine;

using Zenject;

public class MapController : MonoBehaviour {
    [SerializeField] GameObject _globalMapContainer = default;
    [SerializeField] EpisodeInfo[] _episodeInfos = default;
    [SerializeField] float _episodeLength = 0;

    [Inject] LevelController _levelController = default;
    [Inject] DiContainer _diContainer = default;

    List<EpisodeInfo> _existedEpisodeInfos = new List<EpisodeInfo>();
    float _currentOffset;

    void Start() {
        Init();
    }

    void Init() {
        _globalMapContainer.transform.DestroyAllChildren();
        int levelCount = 0;
        foreach (EpisodeInfo episodeInfo in _episodeInfos) {
            var episode = _diContainer
                          .InstantiatePrefab(episodeInfo, new Vector3(_currentOffset, 0, 0), Quaternion.identity,
                                             _globalMapContainer.transform).GetComponent<EpisodeInfo>();

            _currentOffset += _episodeLength;
            _existedEpisodeInfos.Add(episode);
            episode.Init(levelCount);
            levelCount += episode.LevelCount;
            _levelController.AddLevelToList(episode.Levels);
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-_episodeLength / 2, 0), new Vector3(_episodeLength / 2, 0));
    }
}