using System;

using DG.Tweening;

using UnityEngine;

public class UIShineBoosterFX : MonoBehaviour {
    [SerializeField] RectTransform _fxTransform = default;
    [SerializeField] float _topPoint = 1;
    [SerializeField] float _botPoint = -1;
    [SerializeField] float _duration = 0.5f;

    void OnEnable() {
        Reset();
    }

    public void Play() {
        Reset();
        var targetPos = _fxTransform.anchoredPosition;
        targetPos.y = _botPoint;
        _fxTransform.DOAnchorPos(targetPos, _duration).SetId(this);
    }

    void Reset() {
        DOTween.Kill(this);
        var currentPos = _fxTransform.anchoredPosition;
        currentPos.y = _topPoint;
        _fxTransform.anchoredPosition = currentPos;
    }
}