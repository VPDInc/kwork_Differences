using System;
using System.Collections.Generic;
using System.Linq;

using Airion.Extensions;

using Lean.Touch;

using TMPro;

using UnityEngine;

using Zenject;

public class MapController : MonoBehaviour
{
    private const string EPISODE_PREFIX = "Episode ";
    private const float SCREEN_RATIO_PHONE = 1.5f; 

    [SerializeField] private Transform _globalMapContainer = default;

    [Header("Episode length")]
    [SerializeField] private float _episodeLengthPhone = 36.86f;
    [SerializeField] private float _episodeLengthPad = 36.86f;

    [Header("Start camera offset")]
    [SerializeField] private Vector2 _startCameraOffsetPhone;
    [SerializeField] private Vector2 _startCameraOffsetPad;

    [Header("Episode")]
    [SerializeField] [Min(1)] private int _episodeFrontPrespawnCount = 2;
    [SerializeField] private TMP_Text _episodeNumLabel = default;
    [SerializeField] private EpisodeInfo[] _episodeInfos = default;

    [Inject] private LevelController _levelController = default;
    [Inject] private LeanDragCamera _leanDragCamera = default;
    [Inject] private DiContainer _diContainer = default;

    private float _episodeLength;
    private Vector2 _startCameraOffset;
    private Vector3 _scaleEpisode;

    private List<EpisodeInfo> _existedEpisodeInfos = new List<EpisodeInfo>();
    private float _currentOffset;
    private int _episodeCount;
    private int _levelCount;

    void Start()
    {
        EpisodeInfo.EpisodeUnlocked += OnEpisodeUnlocked;

        var screenRatio = (float)Screen.width / Screen.height;
        if (screenRatio < 1) screenRatio = (float)Screen.height / Screen.width;

        if (screenRatio >= SCREEN_RATIO_PHONE)
        {
            _episodeLength = _episodeLengthPhone;
            _startCameraOffset = _startCameraOffsetPhone;
            _scaleEpisode = _episodeInfos[0].transform.localScale;
        }
        else
        {
            _episodeLength = _episodeLengthPad;
            _startCameraOffset = _startCameraOffsetPad;
            _scaleEpisode = Vector3.one;

            _globalMapContainer.position = new Vector3(_globalMapContainer.position.x,
                _globalMapContainer.position.y - 1.25f, _globalMapContainer.position.z);
        }

        _globalMapContainer.DestroyAllChildren();
        _leanDragCamera.SetMinBounds(_startCameraOffset);

        var currentEpisodeNum = _levelController.LastEpisodeNum;
        _levelCount = 0;
        foreach (EpisodeInfo episodeInfo in _episodeInfos)
            SpawnEpisode(episodeInfo);

        if (_episodeCount < currentEpisodeNum + _episodeFrontPrespawnCount)
        {
            for (int i = _episodeCount; i < currentEpisodeNum + _episodeFrontPrespawnCount; i++)
                AddExtraEpisode();
        }
    }

    void Update()
    {
        GetClosestEpisodeNum();
    }

    void OnDestroy()
    {
        EpisodeInfo.EpisodeUnlocked -= OnEpisodeUnlocked; 
    }

    void GetClosestEpisodeNum()
    {
        var closestEpisodeToCamera =
            _existedEpisodeInfos.OrderBy(x => Vector2.SqrMagnitude(x.transform.position -
                                                                _leanDragCamera.transform.position)).First();

        _episodeNumLabel.text = EPISODE_PREFIX + (closestEpisodeToCamera.EpisodeNum + 1);
    }

    void AddExtraEpisode()
    {
        var episodeNumToSpawn = (int)Mathf.Repeat(_episodeCount, _episodeInfos.Length);
        SpawnEpisode(_episodeInfos[episodeNumToSpawn]);
    }
    
    void OnEpisodeUnlocked()
    {
        AddExtraEpisode();
    }

    void SpawnEpisode(EpisodeInfo episodeInfo)
    {
        var postion = new Vector3(_currentOffset, _globalMapContainer.position.y, 0);
        var episode = _diContainer.InstantiatePrefab(episodeInfo, postion,
            Quaternion.identity, _globalMapContainer).GetComponent<EpisodeInfo>();
        episode.transform.localScale = _scaleEpisode;

        _currentOffset += _episodeLength;
        _existedEpisodeInfos.Add(episode);
        episode.Init(_levelCount, _episodeCount);
        _episodeCount++;
        _levelCount += episode.LevelCount;
        _levelController.AddLevelToList(episode.Levels);
        _leanDragCamera.SetMaxBounds(new Vector2(_currentOffset - _episodeLength - _startCameraOffset.x, 0));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-_episodeLength / 2, 0), new Vector3(_episodeLength / 2, 0));
    }
}