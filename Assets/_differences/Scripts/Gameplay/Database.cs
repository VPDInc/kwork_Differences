using System;
using System.Collections.Generic;
using System.IO;

using Airion.Extensions;

using UnityEngine;

public class Database : MonoBehaviour {
    string _dataPath;
    readonly HashSet<string> _completedLevels = new HashSet<string>();
    readonly Dictionary<int, List<Data>> _data = new Dictionary<int, List<Data>>();
    readonly Dictionary<int, List<Data>> _openedData = new Dictionary<int, List<Data>>();
    
    const string SAVE_DATA_PATH = "data.dat";
    const string JSONS_PATH = "Jsons";
    
    void Awake() {
        _dataPath = Path.Combine(Application.persistentDataPath, SAVE_DATA_PATH);
        if (!File.Exists(_dataPath))
            using (File.Create(_dataPath)) { }

        LoadCompletedLevels();
        LoadData();
    }
    
    public Data[] GetData(int dataAmount, int pointsPerData) {
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
}
