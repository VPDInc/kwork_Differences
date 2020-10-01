using System;
using System.Collections;
using System.Collections.Generic;

using DG.Tweening;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;

public class AdsPopup : MonoBehaviour {
    [SerializeField] TMP_Text _titleLabel = default;
    [SerializeField] TMP_Text _descriptionLabel = default;
    [SerializeField] TMP_Text _coinLabel = default;

    UIView _view;

    void Awake() {
        _view = GetComponent<UIView>();
    }

    public void Open(string title, string description, string coinsAmount) {
        _view.Show();
        
        _titleLabel.text = title;
        _descriptionLabel.text = description;
        _coinLabel.text = coinsAmount;
    }

    public void Close(float delay) {
        var seq = DOTween.Sequence();
        seq.AppendInterval(delay);
        seq.AppendCallback(() => {
                               _view.Hide();
                           });
    }
}
