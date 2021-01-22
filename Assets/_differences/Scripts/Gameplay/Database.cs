using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class Database : MonoBehaviour
{
    public enum LoadingStatus
    {
        NotStarted,
        InProgress,
        Success
    }

    private const string JSONS_PATH = "Jsons";
    private const string SAVE_DATA_PATH = "data.dat";

    [SerializeField] private int _superEasyLevels = 5;
    [SerializeField] private LevelImageData[] _images;

    [Inject] private LevelBalanceLibrary _library = default;

    private readonly List<DataLevel> _loadedData = new List<DataLevel>();

    [ShowInInspector, ReadOnly]
    private readonly Dictionary<int, LoadingData> _loadingDatas = new Dictionary<int, LoadingData>();
    private readonly Dictionary<string, Data> _datas = new Dictionary<string, Data>();

    private string _dataPath;

    [ShowInInspector, ReadOnly]
    private List<LevelImageData> _easyImage = new List<LevelImageData>();
    [ShowInInspector, ReadOnly]
    private List<LevelImageData> _normalImage = new List<LevelImageData>();

    private void Awake()
    {
        foreach (var image in _images)
        {
            if (image.Complexity == ComplexityLevel.Normal) _normalImage.Add(image);
            else _easyImage.Add(image);
        }

        _dataPath = Path.Combine(Application.persistentDataPath, SAVE_DATA_PATH);
        if (!File.Exists(_dataPath))
            using (File.Create(_dataPath)) { }

        LoadAllData();
    }

    private void LoadAllData()
    {
        var jsons = Resources.LoadAll<TextAsset>(JSONS_PATH);

        using (StreamReader reader = new StreamReader(_dataPath))
        {
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null) break;

                try
                {
                    var separated = line.Split('|');

                    var nameImage = separated[0];
                    var time = Convert.ToInt64(separated[1]);

                    var countWin = 0;
                    if (separated.Length >= 3) countWin = Convert.ToInt32(separated[2]);
                    else if (time > 0) countWin = 1;

                    var levelData = new DataLevel(nameImage, DateTime.FromBinary(time), countWin);
                    _loadedData.Add(levelData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[{GetType()}] {ex.Message}");
                }
            }
        }

        foreach (var json in jsons)
        {
            var data = DiffUtils.Parse(json.text);
            _datas.Add(data.Id, data);

            bool isEmpty = true;
            foreach (var item in _loadedData)
            {
                if (item.NameLevel == data.Id)
                {
                    isEmpty = false;
                    break;
                }
            }

            if (isEmpty) _loadedData.Add(new DataLevel(data.Id, DateTime.MinValue, 0));
        }
    }

    public void Load(int levelNum)
    {
        var complexity = levelNum > _superEasyLevels ? ComplexityLevel.Normal :
            ComplexityLevel.SuperEasy;

        var balance = _library.GetLevelBalanceInfo(levelNum);
        var datas = GetData(complexity, balance.PictureCount, balance.DifferenceCount);

        StartLoading(levelNum, datas);
    }

    private void StartLoading(int num, Data[] datas)
    {
        if (!_loadingDatas.ContainsKey(num))
            _loadingDatas.Add(num, default);

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

    public LoadingStatus GetLoadingStatus(int levelNum)
    {
        if (!_loadingDatas.ContainsKey(levelNum))
            return LoadingStatus.NotStarted;

        return _loadingDatas[levelNum].Status;
    }

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

    private Data[] GetData(ComplexityLevel complexity, int countImage, int countDifferences)
    {
        var loadedData = _loadedData.Where(d => GetJsonDataById(d.NameLevel).PointCount == countDifferences);
        var levelImageData = complexity == ComplexityLevel.Normal ? _normalImage : _easyImage;
        var imagesList = new List<DataLevel>();

        foreach (var image in levelImageData)
        {
            foreach (var itemData in loadedData)
            {
                if (itemData.NameLevel == "Diff_" + image.NumberImage)
                {
                    imagesList.Add(itemData);
                    break;
                }
            }
        }

        var minOpen = int.MaxValue;
        var maxOpen = int.MinValue;
        foreach (var image in imagesList)
        {
            if (minOpen > image.CountOpen)
                minOpen = image.CountOpen;

            if (maxOpen < image.CountOpen)
                maxOpen = image.CountOpen;
        }

        var countTakenImages = 0;
        var takenImages = new List<DataLevel>();
        var images = imagesList.ToArray();
        ShuffleArray(images);

        while (countTakenImages != countImage)
        {
            foreach (var image in images)
            {
                if (image.CountOpen == minOpen)
                {
                    countTakenImages++;
                    takenImages.Add(image);

                    if (countTakenImages == countImage) break;
                }
            }

            if (countTakenImages != countImage)
            {
                minOpen++;
                if (maxOpen < minOpen)
                {
                    Debug.LogError("Необходимых картинок не найденно");
                    break;
                }
            }
        }

        var outData = new List<Data>();

        for (int i = 0; i < countImage; i++)
        {
            // Can be still not enough data. Just skip it
            if (i < takenImages.Count)
            {
                var data = GetJsonDataById(takenImages[i].NameLevel);
                if (data.Id == String.Empty) continue;

                outData.Add(data);
            }
        }

        return outData.ToArray();
    }

    private void ShuffleArray<T>(T[] array)
    {
        System.Random random = new System.Random();

        for (int i = array.Length - 1; i >= 1; i--)
        {
            int j = random.Next(i + 1);

            var temp = array[j];
            array[j] = array[i];
            array[i] = temp;
        }
    }

    private Data GetJsonDataById(string id)
    {
        if (_datas.TryGetValue(id, out var data)) return data;
        else return default;
    }

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
                if (data.NameLevel.Equals(id))
                {
                    data.CountOpen++;
                    data.Time = DateTime.UtcNow;
                    _loadedData[i] = data;
                }
            }
        }

        ClearFile();

        try
        {
            using (StreamWriter w = File.AppendText(_dataPath))
                foreach (var data in _loadedData)
                    w.WriteLine(data.NameLevel + "|" + data.Time.ToBinary() + "|" + data.CountOpen);
        }
        catch (Exception e)
        {
            Debug.LogError($"[{GetType()}] {e.Message}");
        }
    }

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

    private struct LoadingData
    {
        public Data[] Datas;
        public (Sprite, Sprite)[] Pictures;
        public LoadingStatus Status;
    }

    private struct DataLevel
    {
        private string _nameLevel;
        private DateTime _time;
        private int _countOpen;

        public string NameLevel => _nameLevel;

        public DateTime Time
        {
            get => _time;
            set => _time = value;
        }

        public int CountOpen
        {
            get => _countOpen;
            set
            {
                if (value > _countOpen)
                    _countOpen = value;
            }
        }

        public DataLevel(string nameLevel, DateTime time, int countOpen)
        {
            _nameLevel = nameLevel;
            _time = time;
            _countOpen = countOpen;
        }
    }
}
