using System;
using System.Collections;
using System.Collections.Generic;

using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

public class DiffVisualFX : MonoBehaviour {
    [SerializeField] Image _mask = default;
    [SerializeField] float _fxDuration = 1f;

    void Start() {
        _mask.fillAmount = 0;
        ShowFx();
    }

    void ShowFx() {
        _mask.DOFillAmount(1, _fxDuration).SetId(this);
    }

    void OnDestroy() {
        DOTween.Kill(this);
    }
}