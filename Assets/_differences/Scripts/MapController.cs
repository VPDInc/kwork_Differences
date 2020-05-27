using UnityEngine;

public class MapController : MonoBehaviour {
    [SerializeField] GameObject _globalMapContainer = default;

    EpisodeInfo[] _episodeInfos;

    void Start() {
        Init();
    }

    void Init() {
        _episodeInfos = _globalMapContainer.GetComponentsInChildren<EpisodeInfo>();
        int levelCount = 0;
        foreach (EpisodeInfo episodeInfo in _episodeInfos) {
            episodeInfo.Init(levelCount);
            levelCount += episodeInfo.LevelCount;
        }
    }
}