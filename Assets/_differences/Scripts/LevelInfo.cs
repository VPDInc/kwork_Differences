using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

public class LevelInfo : MonoBehaviour {
    [SerializeField] TMP_Text _levelNumLabel = default;

    public void Setup(int levelNum) {
        _levelNumLabel.text = levelNum.ToString();
    }
}