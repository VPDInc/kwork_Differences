using System;
using System.Collections.Generic;
using System.Linq;

using Airion.Extensions;

using Lean.Touch;

using TMPro;

using UnityEngine;

using Zenject;

public class MapController : MonoBehaviour {
    [SerializeField] GameObject _globalMapContainer = default;
    [SerializeField] EpisodeInfo[] _episodeInfos = default;
    [SerializeField] float _episodeLength = 0;
    [SerializeField] int _episodeFrontPrespawnCount = 2;
    [SerializeField] TMP_Text _episodeNumLabel = default;

    [Inject] LevelController _levelController = default;
    [Inject] LeanDragCamera _leanDragCamera = default;
    [Inject] DiContainer _diContainer = default;

    List<EpisodeInfo> _existedEpisodeInfos = new List<EpisodeInfo>();
    float _currentOffset;
    int _episodeCount;
    int _levelCount;

    const string EPISODE_PREFIX = "Episode ";

    void Start() {
        EpisodeInfo.EpisodeUnlocked += OnEpisodeUnlocked; 
        Init();
    }

    void Update() {
        GetClosestEpisodeNum();
    }

    void OnDestroy() {
        EpisodeInfo.EpisodeUnlocked -= OnEpisodeUnlocked; 
    }

    void Init() {
        _globalMapContainer.transform.DestroyAllChildren();
        
        _leanDragCamera.SetMinBounds(new Vector2(_currentOffset, 0));
        
        var currentEpisodeNum = _levelController.LastEpisodeNum;
        _levelCount = 0;
        foreach (EpisodeInfo episodeInfo in _episodeInfos) {
            SpawnEpisode(episodeInfo);
        }

        if (_episodeCount < currentEpisodeNum + _episodeFrontPrespawnCount) {
            for (int i = _episodeCount; i < currentEpisodeNum + _episodeFrontPrespawnCount; i++) {
                AddExtraEpisode();
            }
        }
    }

    void GetClosestEpisodeNum() {
        var closestEpisodeToCamera =
            _existedEpisodeInfos.OrderBy(x => Vector2.SqrMagnitude(x.transform.position -
                                                                _leanDragCamera.transform.position)).First();

        _episodeNumLabel.text = EPISODE_PREFIX + (closestEpisodeToCamera.EpisodeNum + 1);
    }

    void AddExtraEpisode() {
        var episodeNumToSpawn = (int)Mathf.Repeat(_episodeCount, _episodeInfos.Length);
        SpawnEpisode(_episodeInfos[episodeNumToSpawn]);
    }
    
    void OnEpisodeUnlocked() {
        AddExtraEpisode();
    }

    void SpawnEpisode(EpisodeInfo episodeInfo) {
        var episode = _diContainer
                      .InstantiatePrefab(episodeInfo, new Vector3(_currentOffset, 0, 0), Quaternion.identity,
                                         _globalMapContainer.transform).GetComponent<EpisodeInfo>();

        _currentOffset += _episodeLength;
        _existedEpisodeInfos.Add(episode);
        episode.Init(_levelCount, _episodeCount);
        _episodeCount++;
        _levelCount += episode.LevelCount;
        _levelController.AddLevelToList(episode.Levels);
        _leanDragCamera.SetMaxBounds(new Vector2(_currentOffset - _episodeLength, 0));
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-_episodeLength / 2, 0), new Vector3(_episodeLength / 2, 0));
    }
}