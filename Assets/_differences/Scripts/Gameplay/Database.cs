using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Airion.Extensions;

using UnityEngine;

using Zenject;

public class Database : MonoBehaviour {
    string _dataPath;
    readonly HashSet<string> _completedLevels = new HashSet<string>();
    readonly Dictionary<int, List<Data>> _data = new Dictionary<int, List<Data>>();
    readonly Dictionary<int, List<Data>> _openedData = new Dictionary<int, List<Data>>();
    
    const string SAVE_DATA_PATH = "data.dat";
    const string JSONS_PATH = "Jsons";

    [Inject] LevelBalanceLibrary _library = default;
    
    readonly Dictionary<int, LoadingData> _loadingDatas = new Dictionary<int, LoadingData>();

    struct LoadingData {
        public Data[] Datas;
        public (Sprite, Sprite)[] Pictures;
        public LoadingStatus Status;
    }
    
    void Awake() {
        _dataPath = Path.Combine(Application.persistentDataPath, SAVE_DATA_PATH);
        if (!File.Exists(_dataPath))
            using (File.Create(_dataPath)) { }

        LoadCompletedLevels();
        LoadData();
    }

    public enum LoadingStatus {
        NotStarted,
        InProgress,
        Success
    }

    public void Load(int levelNum) {
        var balanceInfo = _library.GetLevelBalanceInfo(levelNum);
        var datas = GetData(balanceInfo.PictureCount, balanceInfo.DifferenceCount);
        StartLoading(levelNum, datas);
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

        return _loadingDatas[levelNum].Pictures;
    }
    
    Data[] GetData(int dataAmount, int pointsPerData) {
        var outData = new List<Data>();
        
        for (int i = 0; i < dataAmount; i++) {
            if (FindBy(pointsPerData, _openedData, out var data)) {
                outData.Add(data);
            } else if (FindBy(pointsPerData, _data, out var nextData)) {
                outData.Add(nextData);
            }
        }

        return outData.ToArray();
    }

    bool FindBy(int pointsPerData, Dictionary<int, List<Data>> dict, out Data outData) {
        const int UP_LIMIT = 10;
        var currentAmount = pointsPerData;
        while (currentAmount <= UP_LIMIT) {
            if (dict.ContainsKey(currentAmount)) {
                outData = dict[currentAmount].RandomElement();
                SaveLevel(outData);
                return true;
            }
            currentAmount++;
        } 
        
        currentAmount = pointsPerData-1;
        while (currentAmount >= 0) {
            if (dict.ContainsKey(currentAmount)) {
                outData = dict[currentAmount].RandomElement();
                SaveLevel(outData);
                return true;
            }
            currentAmount--;
        }

        outData = default;
        return false;
    }

    void LoadData() {
        var jsons = Resources.LoadAll<TextAsset>(JSONS_PATH);
        foreach (var json in jsons) {
            var data = DiffUtils.Parse(json.text);
            var pointsAmount = data.Points.Length;
            if (!_data.ContainsKey(pointsAmount))
                _data.Add(pointsAmount, new List<Data>());
            _data[pointsAmount].Add(data);

            if (!_completedLevels.Contains(data.Id)) {
                if (!_openedData.ContainsKey(pointsAmount))
                    _openedData.Add(pointsAmount, new List<Data>());
                _openedData[pointsAmount].Add(data);
            }
        }
    }

    void SaveLevel(Data data) {
        SaveLevels(new []{data.Id});
    }

    void SaveLevels(IEnumerable<string> ids) {
        try {
            using (StreamWriter w = new StreamWriter(_dataPath)) {
                foreach (var id in ids) {
                    _completedLevels.Add(id);
                    w.WriteLine(id);
                }
            }
        } catch (Exception e) {
            Debug.LogError($"[{GetType()}] {e.Message}");
        }
    }

    void LoadCompletedLevels() {
        try {
            using (StreamReader r = new StreamReader(_dataPath)) {
                while (r.Peek() >= 0) {
                    var line = r.ReadLine();
                    _completedLevels.Add(line);
                }
            }
        } catch (Exception e) {
            Debug.LogError($"[{GetType()}] {e.Message}");
        }
    }

    [ContextMenu("Clear")]
    void ClearLevels() {
        _completedLevels.Clear();
        _data.Clear();
        _openedData.Clear();
            
        if (File.Exists(_dataPath))
            File.Delete(_dataPath);
        
        if (!File.Exists(_dataPath))
            using (File.Create(_dataPath)) { }
        
        LoadData();
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
