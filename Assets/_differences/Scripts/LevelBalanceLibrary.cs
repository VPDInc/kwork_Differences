using System;
using System.Collections.Generic;

using GoogleSheetsToUnity;

using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;

#endif

[CreateAssetMenu(fileName = "LevelBalanceInfo", menuName = "ScriptableObjects/Create LevelBalanceInfo", order = 1)]
public class LevelBalanceLibrary : ScriptableObject {
    public string AssociatedSheet = "17NDb4vAhBR8-XUrxEDFAkcxJIX6D3mbYyIR47KwYS9I";
    public string AssociatedWorksheet = "Levels";

    [Serializable]
    public struct LevelBalanceInfo {
        public int PictureCount;
        public int DifferenceCount;
    }

    [SerializeField]
    public List<LevelBalanceInfo> LevelBalanceInfos = new List<LevelBalanceInfo>();

    public LevelBalanceInfo GetLevelBalanceInfo(int levelNum) {
        return LevelBalanceInfos[levelNum];
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelBalanceLibrary))]
public class LevelBalancedInfoEditor : Editor {
    LevelBalanceLibrary _levelBalanceLibrary;

    void OnEnable() {
        _levelBalanceLibrary = (LevelBalanceLibrary) target;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Update")) {
            UpdateStats();
        }
    }

    void UpdateStats() {
        SpreadsheetManager
            .Read(new GSTU_Search(_levelBalanceLibrary.AssociatedSheet, _levelBalanceLibrary.AssociatedWorksheet),
                  UpdateAllLevels);
    }

    void UpdateAllLevels(GstuSpreadSheet spreadsheetRef) {
        _levelBalanceLibrary.LevelBalanceInfos.Clear();
        var rows = spreadsheetRef.rows.primaryDictionary;
        for (int i = 2; i <= rows.Count; i++) {
            var levelBalanceInfo = new LevelBalanceLibrary.LevelBalanceInfo();
            foreach (GSTU_Cell cell in rows[i]) {
                switch (cell.columnId) {
                    case "PictureCount":
                        levelBalanceInfo.PictureCount = int.Parse(cell.value);
                        break;
                    case "DifferencesCount":
                        levelBalanceInfo.DifferenceCount = int.Parse(cell.value);
                        break;
                }
            }
            _levelBalanceLibrary.LevelBalanceInfos.Add(levelBalanceInfo);
        }
    }
}
#endif