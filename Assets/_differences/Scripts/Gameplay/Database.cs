using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _differences.Scripts.Extension;
using Sirenix.OdinInspector;

using UnityEngine;

using Zenject;

public class Database : MonoBehaviour
{
    private const string SAVE_DATA_PATH = "data.dat";
    private const string JSONS_PATH = "Jsons";

    [SerializeField] private int _firstLevels = 5;
    [SerializeField] private string[] _firstIds = default;

    readonly private Dictionary<string, Data> _datas = new Dictionary<string, Data>();

    [ShowInInspector, ReadOnly]
    readonly private List<(string, DateTime)> _loadedData = new List<(string, DateTime)>();
    
    [ShowInInspector, ReadOnly]
    [Inject] private LevelBalanceLibrary _library = default;

    private List<string> _ordered => _loadedData.OrderBy(d => d.Item2).Select(d => d.Item1 + " " + d.Item2.ToString()).ToList();
    readonly private Dictionary<int, LoadingData> _loadingDatas = new Dictionary<int, LoadingData>();

    private string _dataPath;

    //TODO 24.01.2021 NEED CREATE JSON SAVE
    private void Awake()
    {
        _dataPath = Path.Combine(Application.persistentDataPath, SAVE_DATA_PATH);

        if (!File.Exists(_dataPath))
            using (File.Create(_dataPath)) { }

        LoadAllData();
    }

    #region GamePlay
    public Data[] GetData(int levelNum)
    {
        if (!_loadingDatas.ContainsKey(levelNum))
        {
            Debug.LogError($"[{GetType()}] Load data '{levelNum}' first!");
            return null;
        }

        return _loadingDatas[levelNum].Datas;
    }

    public (Sprite, Sprite)[] GetPictures(int levelNum)
    {
        if (!_loadingDatas.ContainsKey(levelNum))
        {
            Debug.LogError($"[{GetType()}] Load data '{levelNum}' first!");
            return null;
        }

        SaveLevels(_loadingDatas[levelNum].Datas);

        return _loadingDatas[levelNum].Pictures;
    }

    public LoadingStatus GetLoadingStatus(int levelNum)
    {
        if (!_loadingDatas.ContainsKey(levelNum))
            return LoadingStatus.NotStarted;

        return _loadingDatas[levelNum].Status;
    }

    #endregion

    #region Load

    public void Load(int levelNum)
    {
        var balanceInfo = _library.GetLevelBalanceInfo(levelNum);

        StartLoading(levelNum, GetData(balanceInfo.PictureCount, balanceInfo.DifferenceCount, levelNum));
    }

    private void LoadAllData()
    {
        var completed = LoadClosedLevels();
        var jsons = Resources.LoadAll<TextAsset>(JSONS_PATH);
        foreach (var json in jsons)
        {
            var data = DiffUtils.Parse(json.text);
            _datas.Add(data.Id, data);
            if (completed.TryGetValue(data.Id, out var date))
            {
                _loadedData.Add((data.Id, date));
            }
            else
            {
                _loadedData.Add((data.Id, DateTime.MinValue));
            }
        }
    }

    private Dictionary<string, DateTime> LoadClosedLevels()
    {
        var completedLevels = new Dictionary<string, DateTime>();
        try
        {
            using (StreamReader r = new StreamReader(_dataPath))
            {
                while (r.Peek() >= 0)
                {
                    var line = r.ReadLine();
                    try
                    {
                        var separated = line.Split('|');
                        // Debug.Log(separated[0] + " " + separated[1]);
                        var buffer = Convert.ToInt64(separated[1]);
                        // Debug.Log(buffer);
                        completedLevels.Add(separated[0], DateTime.FromBinary(buffer));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[{GetType()}] {e.Message}");
                        // just nothing to do  
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[{GetType()}] {e.Message}");
        }

        return completedLevels;
    }

    private void StartLoading(int num, Data[] datas)
    {
        if (!_loadingDatas.ContainsKey(num))
        {
            _loadingDatas.Add(num, default);
        }

        StartCoroutine(Loading(num, datas));
    }

    private IEnumerator Loading(int num, Data[] datas)
    {
        var loadingData = new LoadingData()
        {
            Datas = datas,
            Status = LoadingStatus.InProgress,
            Pictures = new (Sprite, Sprite)[datas.Length]
        };

        _loadingDatas[num] = loadingData;

        var loader = new Loader(this);
        for (var index = 0; index < loadingData.Datas.Length; index++)
        {
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

    private struct Levels
    {
        public string Id;
        public DateTime TimeToOpen;
    }

    private Data[] GetData(int dataAmount, int pointsPerData, int levelNum)
    {
        var outData = new List<Data>();
        var levels = new List<Levels>();

        if (levelNum <= _firstLevels)
        {
            var openedSymply = _loadedData.Where(d => _firstIds.Contains(d.Item1)).
                Where(x=> GetJsonDataById(x.Item1).PointCount == pointsPerData).ToArray();

            ShuffleList(levels, openedSymply);
        }
        else
        {
            var opened = _loadedData.Where(d => GetJsonDataById(d.Item1).PointCount == pointsPerData).ToArray();

            ShuffleList(levels, opened);
        }

        levels.Sort((x, y) => x.TimeToOpen.CompareTo(y.TimeToOpen));
            
        for (int i = 0; i < dataAmount; i++)
        {
            if (i < levels.Count)
            {
                var data = GetJsonDataById(levels[i].Id);
                if (data.Id == String.Empty)
                    continue;

                outData.Add(data);
            }
        }

        return outData.ToArray();

        void ShuffleList(List<Levels> level, (string, DateTime)[] opened)
        {
            for (int i = 0; i < opened.Length; i++)
                level.Add(new Levels { Id = opened[i].Item1, TimeToOpen = opened[i].Item2 });

            level.Shuffle();
        }
    }

    private Data GetJsonDataById(string id)
    {
        if (_datas.TryGetValue(id, out var data))
        {
            return data;
        }
        else return default;
    }

    private struct LoadingData
    {
        public Data[] Datas;
        public (Sprite, Sprite)[] Pictures;
        public LoadingStatus Status;
    }

    public enum LoadingStatus
    {
        NotStarted,
        InProgress,
        Success
    }

#if UNITY_EDITOR
    public bool LoadSpecificJson(int i)
    {
        var path = UnityEditor.EditorUtility.OpenFilePanel("Load file", "Assets/Resources/Jsons", "json");
        if (!File.Exists(path))
        {
            return false;
        }
        var jsonString = File.ReadAllText(path);
        var data = DiffUtils.Parse(jsonString);
        StartLoading(i, new[] { data });
        return true;
    }
#endif

    #endregion

    #region Save

    private void SaveLevels(IEnumerable<Data> data)
    {
        SaveLevels(data.Select(d => d.Id));
    }

    // TODO: this is kind of shit. Just need to use sql bd
    private void SaveLevels(IEnumerable<string> ids)
    {
        foreach (var id in ids)
        {
            for (int i = 0; i < _loadedData.Count; i++)
            {
                var data = _loadedData[i];
                if (data.Item1.Equals(id))
                {
                    data.Item2 += new TimeSpan(0,1,0);
                    _loadedData[i] = data;
                }
            }
        }

        ClearFile();

        try
        {
            using (StreamWriter w = File.AppendText(_dataPath))
            {
                foreach (var data in _loadedData)
                {
                    w.WriteLine(data.Item1 + "|" + data.Item2.ToBinary());
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[{GetType()}] {e.Message}");
        }
    }
    #endregion

    #region Clear
    [ContextMenu("Clear")]
    private void ClearLevels()
    {
        PlayerPrefs.DeleteAll();

        if (File.Exists(_dataPath))
            File.Delete(_dataPath);

        if (!File.Exists(_dataPath))
            using (File.Create(_dataPath)) { }
    }

    private void ClearFile()
    {
        if (File.Exists(_dataPath))
            File.Delete(_dataPath);

        if (!File.Exists(_dataPath))
            using (File.Create(_dataPath)) { }
    }

    #endregion

}
