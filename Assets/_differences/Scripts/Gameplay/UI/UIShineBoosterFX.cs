using System;

using DG.Tweening;

using UnityEngine;

public class UIShineBoosterFX : MonoBehaviour {
    [SerializeField] float _topPoint = 1;
    [SerializeField] float _botPoint = -1;
    [SerializeField] float _duration = 0.5f;
    [SerializeField] float _scaleModifier = 1.1f;
    [SerializeField] int _scaleBeats = 1;
    
    
    RectTransform _fxTransform = default;

    void Awake() {
        _fxTransform = GetComponent<RectTransform>();
    }

    void OnEnable() {
        Reset();
    }

    public void Play() {
        Reset();
        var targetPos = _fxTransform.anchoredPosition;
        targetPos.y = _botPoint;
        _fxTransform.DOAnchorPos(targetPos, _duration).SetId(this);
        _fxTransform.parent.DOScale(_scaleModifier, _duration / 2).SetLoops(2 * _scaleBeats, LoopType.Yoyo);
    }

    void Reset() {
        DOTween.Kill(this);
        var currentPos = _fxTransform.anchoredPosition;
        currentPos.y = _topPoint;
        _fxTransform.anchoredPosition = currentPos;
    }
}