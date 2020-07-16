using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

public class RoundInfo : MonoBehaviour {
    [SerializeField] TMP_Text _roundLabel = default;
    [SerializeField] TMP_Text _pointsLabel = default;

    public void Setup(string roundText, string pointsText) {
        _roundLabel.text = roundText;
        _pointsLabel.text = pointsText;
    }
}