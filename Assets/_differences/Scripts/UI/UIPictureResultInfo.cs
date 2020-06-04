using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class UIPictureResultInfo : MonoBehaviour {
    [SerializeField] UIDifferencesCounterResult _differencesCounterResultPrefab = default;
    [SerializeField] Transform _differencesCounterHolder = default;
    [SerializeField] TMP_Text _scoreLabel = default;

    public void Setup(List<DifferencePoint> differencePoints, int score) {
        for (int i = 0; i < differencePoints.Count; i++) {
            var differencesResult = Instantiate(_differencesCounterResultPrefab, _differencesCounterHolder);
            differencesResult.Setup(differencePoints[i].IsOpen);
        }

        _scoreLabel.text = score.ToString();
    }
}