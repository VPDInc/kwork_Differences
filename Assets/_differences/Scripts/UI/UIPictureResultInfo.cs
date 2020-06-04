using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class UIPictureResultInfo : MonoBehaviour {
    [SerializeField] UIDifferencesCounterResult _differencesCounterResultPrefab = default;
    [SerializeField] Transform _differencesCounterHolder = default;
    [SerializeField] TMP_Text _scoreLabel = default;

    public void Setup(Dictionary<int, bool> differencesInfo, int score) {
        for (int i = 0; i < differencesInfo.Count; i++) {
            var differencesResult = Instantiate(_differencesCounterResultPrefab, _differencesCounterHolder);
            differencesResult.Setup(differencesInfo[i]);
        }

        _scoreLabel.text = score.ToString();
    }
}