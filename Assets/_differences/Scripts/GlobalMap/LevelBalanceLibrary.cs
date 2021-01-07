using System;
using System.Collections.Generic;

using GoogleSheetsToUnity;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[CreateAssetMenu(fileName = "LevelBalanceInfo", menuName = "Differences/Create LevelBalanceInfo", order = 1)]
public class LevelBalanceLibrary : ScriptableObject {
    [Header("Table Info")] [SerializeField]
    string _associatedSheet = "18UG0Qh1AmI8oxMm748NVtsaXjTPWjGWQRh52-mY-oSI";
    [SerializeField] string _associatedWorksheet = "Levels";

    [Header("Level Handling Info")] [SerializeField]
    int _repeatAfter = 10;

    public string AssociatedSheet => _associatedSheet;
    public string AssociatedWorksheet => _associatedWorksheet;

    [Serializable]
    public struct LevelBalanceInfo {
        public int PictureCount;
        public int DifferenceCount;
    }

    [Header("Level Library")] [SerializeField]
    public List<LevelBalanceInfo> LevelBalanceInfos = new List<LevelBalanceInfo>();

    public LevelBalanceInfo GetLevelBalanceInfo(int levelNum) {
        var levelToLoad = levelNum;
        if (levelToLoad >= LevelBalanceInfos.Count) {
            var repeatableAmount = LevelBalanceInfos.Count - _repeatAfter;
            var partingLevel = levelToLoad % repeatableAmount;
            levelToLoad = _repeatAfter + partingLevel;
        }

        return LevelBalanceInfos[levelToLoad];
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
        Debug.Log("Tut");
        SpreadsheetManager
            .Read(new GSTU_Search(_levelBalanceLibrary.AssociatedSheet, _levelBalanceLibrary.AssociatedWorksheet),
                  UpdateAllLevels);
    }

    void UpdateAllLevels(GstuSpreadSheet spreadsheetRef) {
        _levelBalanceLibrary.LevelBalanceInfos.Clear();
        var rows = spreadsheetRef.rows.primaryDictionary;

        Debug.Log(rows.Count);

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