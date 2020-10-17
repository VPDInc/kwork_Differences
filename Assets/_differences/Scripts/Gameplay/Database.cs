using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

using Zenject;

public class Database : MonoBehaviour {
    [SerializeField] int _firstLevels = 5;
    [SerializeField] string[] _firstIds = default;
    
    [Inject] LevelBalanceLibrary _library = default;
    
    string _dataPath;

    readonly List<Data> _pool = new List<Data>();
    readonly List<Data> _openedData = new List<Data>(); 
    readonly Dictionary<int, LoadingData> _loadingDatas = new Dictionary<int, LoadingData>();
    
    const string SAVE_DATA_PATH = "data.dat";
    const string JSONS_PATH = "Jsons";

    struct LoadingData {
        public Data[] Datas;
        public (Sprite, Sprite)[] Pictures;
        public LoadingStatus Status;
    }
    
    void Awake() {
        _dataPath = Path.Combine(Application.persistentDataPath, SAVE_DATA_PATH);
        if (!File.Exists(_dataPath))
            using (File.Create(_dataPath)) { }

        LoadAllData();
    }

    public enum LoadingStatus {
        NotStarted,
        InProgress,
        Success
    }

    public void Load(int levelNum) {
        Data[] datas = null;
        var balanceInfo = _library.GetLevelBalanceInfo(levelNum);
        if (levelNum <= _firstLevels) {
            datas = GetSimplifiedData(balanceInfo.PictureCount, balanceInfo.DifferenceCount);
        } else {
            datas = GetData(balanceInfo.PictureCount, balanceInfo.DifferenceCount);
        }
        
        StartLoading(levelNum, datas);
    }

    void LoadAllData() {
        var completed = LoadClosedLevels();
        var jsons = Resources.LoadAll<TextAsset>(JSONS_PATH);
        foreach (var json in jsons) {
            var data = DiffUtils.Parse(json.text);
            if (completed.Contains(data.Id)) {
                _pool.Add(data);
            } else {
                _openedData.Add(data);                    
            } 
        }
    }
    
    HashSet<string> LoadClosedLevels() {
        var completedLevels = new HashSet<string>();
        try {
            using (StreamReader r = new StreamReader(_dataPath)) {
                while (r.Peek() >= 0) {
                    var line = r.ReadLine();
                    completedLevels.Add(line);
                }
            }
        } catch (Exception e) {
            Debug.LogError($"[{GetType()}] {e.Message}");
        }

        return completedLevels;
    }


    void StartLoading(int num, Data[] datas) {
        if (!_loadingDatas.ContainsKey(num)) {
            _loadingDatas.Add(num, default);
        }

        StartCoroutine(Loading(num, datas));
    }

    IEnumerator Loading(int num, Data[] datas) {
        var loadingData = new LoadingData() {
            Datas = datas,
            Status = LoadingStatus.InProgress,
            Pictures = new (Sprite, Sprite)[datas.Length]
        };

        _loadingDatas[num] = loadingData;
        
        var loader = new Loader(this);
        for (var index = 0; index < loadingData.Datas.Length; index++) {
            var data = loadingData.Datas[index];
            var path1 = data.Image1Path;
            var path2 = data.Image2Path;
            
            yield return loader.Run(path1, path2, data.Storage);
            
            loadingData.Pictures[index].Item1 = loader.Result.Item1;
            loadingData.Pictures[index].Item2 = loader.Result.Item2;
        }

        loadingData.Status = LoadingStatus.Success;
        _loadingDatas[num] = loadingData;
    }
    
    public LoadingStatus GetLoadingStatus(int levelNum) {
        if (!_loadingDatas.ContainsKey(levelNum))
            return LoadingStatus.NotStarted;

        return _loadingDatas[levelNum].Status;
    }

    public Data[] GetData(int levelNum) {
        if (!_loadingDatas.ContainsKey(levelNum)) {
            Debug.LogError($"[{GetType()}] Load data '{levelNum}' first!");
            return null;
        }

        return _loadingDatas[levelNum].Datas;
    }
    
    public (Sprite, Sprite)[] GetPictures(int levelNum) {
        if (!_loadingDatas.ContainsKey(levelNum)) {
            Debug.LogError($"[{GetType()}] Load data '{levelNum}' first!");
            return null;
        }
        
        SaveLevels(_loadingDatas[levelNum].Datas);
        return _loadingDatas[levelNum].Pictures;
    }
    
    Data[] GetSimplifiedData(int dataAmount, int pointsPerData) {
        var opened = _openedData.Where(d => _firstIds.Contains(d.Id)).ToArray();
        
        var outData = new List<Data>();
        // if cant find enough data in opened pool just load completed pool as well 
        if (opened.Length < dataAmount) {
            _openedData.AddRange(_pool);
            ClearSavedData();
            opened = _openedData.Where(d => _firstIds.Contains(d.Id)).ToArray();
        }
        
        for (int i = 0; i < dataAmount; i++) {
            // can be still not enough data. Just skip it
            if (i < opened.Length) {
                var data = opened[i];
                outData.Add(data);
                _pool.Add(data);
                _openedData.Remove(data);
            }
        }
        
        return outData.ToArray();
    }
    
    Data[] GetData(int dataAmount, int pointsPerData) {
        var opened = _openedData.Where(d => d.PointCount == pointsPerData).ToArray();
        // if cant find enough data in opened pool just load completed pool as well 
        if (opened.Length < dataAmount) {
            _openedData.AddRange(_pool);
            ClearSavedData();
            opened = _openedData.Where(d => d.PointCount == pointsPerData).ToArray();
        }

        var outData = new List<Data>();

        for (int i = 0; i < dataAmount; i++) {
            // can be still not enough data. Just skip it
            if (i < opened.Length) {
                var data = opened[i];
                outData.Add(data);
                _pool.Add(data);
                _openedData.Remove(data);
            }
        }
        
        return outData.ToArray();
    }

    void SaveLevel(Data data) {
        SaveLevels(new []{data.Id});
    }
    
    void SaveLevels(IEnumerable<Data> data) {
        SaveLevels(data.Select(d => d.Id));
    }
    
    void SaveLevels(IEnumerable<string> ids) {
        try {
            using (StreamWriter w = File.AppendText(_dataPath)) {
                foreach (var id in ids) {
                    w.WriteLine(id);
                }
            }
        } catch (Exception e) {
            Debug.LogError($"[{GetType()}] {e.Message}");
        }
    }

    [ContextMenu("Clear")]
    void ClearLevels() {
        _openedData.Clear();
            
        if (File.Exists(_dataPath))
            File.Delete(_dataPath);
        
        if (!File.Exists(_dataPath))
            using (File.Create(_dataPath)) { }
    }

    void ClearSavedData() {
        _pool.Clear();
        if (File.Exists(_dataPath))
            File.Delete(_dataPath);
        
        if (!File.Exists(_dataPath))
            using (File.Create(_dataPath)) { }
    }

    #if UNITY_EDITOR
    public bool LoadSpecificJson(int i) {
        var path = UnityEditor.EditorUtility.OpenFilePanel("Load file", "Assets/Resources/Jsons", "json");
        if (!File.Exists(path)) {
            return false;
        }
        var jsonString = File.ReadAllText(path);
        var data = DiffUtils.Parse(jsonString);
        StartLoading(i, new [] {data});
        return true;
    }
    #endif
}
