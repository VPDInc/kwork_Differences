using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

using Zenject;

public class Database : MonoBehaviour {
    [SerializeField] int _firstLevels = 5;
    [SerializeField] string[] _firstIds = default;
    
    [Inject] LevelBalanceLibrary _library = default;
    
    string _dataPath;

    readonly Dictionary<string, Data> _datas = new Dictionary<string, Data>();
    [ShowInInspector, ReadOnly]
    readonly List<(string, DateTime)> _loadedData = new List<(string, DateTime)>();
    [ShowInInspector, ReadOnly]
    List<string> _ordered => _loadedData.OrderBy(d => d.Item2).Select(d => d.Item1 + " " + d.Item2.ToString()).ToList();
    readonly Dictionary<int, LoadingData> _loadingDatas = new Dictionary<int, LoadingData>();
    
    const string SAVE_DATA_PATH = "data.dat";
    const string JSONS_PATH = "Jsons";

    struct LoadingData {
        public Data[] Datas;
        public (Sprite, Sprite)[] Pictures;
        public LoadingStatus Status;
    }
    
    void Awake() {
        //PlayerPrefs.DeleteAll();
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
            _datas.Add(data.Id, data);
            if (completed.TryGetValue(data.Id, out var date)) {
                _loadedData.Add((data.Id, date));
            } else {
                _loadedData.Add((data.Id, DateTime.MinValue));
            } 
        }
    }
    
    Dictionary<string, DateTime> LoadClosedLevels() {
        var completedLevels = new Dictionary<string, DateTime>();
        try {
            using (StreamReader r = new StreamReader(_dataPath)) {
                while (r.Peek() >= 0) {
                    var line = r.ReadLine();
                    try {
                        var separated = line.Split('|');
                        // Debug.Log(separated[0] + " " + separated[1]);
                        var buffer = Convert.ToInt64(separated[1]);
                        // Debug.Log(buffer);
                        completedLevels.Add(separated[0], DateTime.FromBinary(buffer));
                    } catch(Exception e) {
                        Debug.LogError($"[{GetType()}] {e.Message}");
                      // just nothing to do  
                    }
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
        var opened = _loadedData.Where(d => _firstIds.Contains(d.Item1)).OrderBy(d => d.Item2).ToArray();

        var outData = new List<Data>();
        
        for (int i = 0; i < dataAmount; i++) {
            // can be still not enough data. Just skip it
            if (i < opened.Length) {
                var data = GetJsonDataById(opened[i].Item1);
                if (data.Id == String.Empty)
                    continue;
                
                outData.Add(data);
            }
        }

        SetSelectionTimeForLevels(outData.Select(d => d.Id));
        return outData.ToArray();
    }

    Data GetJsonDataById(string id) {
        if (_datas.TryGetValue(id, out var data)) {
            return data;
        } else return default;
    }
    
    Data[] GetData(int dataAmount, int pointsPerData) {
        var opened = _loadedData.Where(d => GetJsonDataById(d.Item1).PointCount == pointsPerData).OrderBy(d => d.Item2).ToArray();

        var outData = new List<Data>();

        for (int i = 0; i < dataAmount; i++) {
            // can be still not enough data. Just skip it
            if (i < opened.Length) {
                var data = GetJsonDataById(opened[i].Item1);
                if (data.Id == String.Empty)
                    continue;
                
                outData.Add(data);
            }
        }
        
        SetSelectionTimeForLevels(outData.Select(d => d.Id));
        return outData.ToArray();
    }

    void SaveLevel(Data data) {
        SaveLevels(new []{data.Id});
    }
    
    void SaveLevels(IEnumerable<Data> data) {
        SaveLevels(data.Select(d => d.Id));
    }
    
    // TODO: this is kind of shit. Just need to use sql bd
    void SaveLevels(IEnumerable<string> ids) {
        foreach (var id in ids) {
            for (int i = 0; i < _loadedData.Count; i++) {
                var data = _loadedData[i];
                if (data.Item1.Equals(id)) {
                    data.Item2 = DateTime.UtcNow;
                    _loadedData[i] = data;
                }
            }
        }
        
        ClearFile();
        
        try {
            using (StreamWriter w = File.AppendText(_dataPath)) {
                foreach (var data in _loadedData) {
                    w.WriteLine(data.Item1 + "|" + data.Item2.ToBinary());
                }
            }
        } catch (Exception e) {
            Debug.LogError($"[{GetType()}] {e.Message}");
        }
    }

    void SetSelectionTimeForLevels(IEnumerable<string> ids) {
        foreach (var id in ids) {
            for (int i = 0; i < _loadedData.Count; i++) {
                var data = _loadedData[i];
                if (data.Item1.Equals(id)) {
                    data.Item2 = DateTime.UtcNow;
                    _loadedData[i] = data;
                }
            }
        }
    }

    [ContextMenu("Clear")]
    void ClearLevels() {
        if (File.Exists(_dataPath))
            File.Delete(_dataPath);
        
        if (!File.Exists(_dataPath))
            using (File.Create(_dataPath)) { }
    }

    void ClearFile() {
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
