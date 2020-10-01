using System;

using DG.Tweening;

using Sirenix.OdinInspector;

using UnityEngine;

using Random = UnityEngine.Random;

public class UIShineBoosterFX : MonoBehaviour {
    [SerializeField] float _topPoint = 1;
    [SerializeField] float _botPoint = -1;
    [SerializeField] float _duration = 0.5f;
    [SerializeField] bool _isScaleChanging = true;
    [SerializeField, ShowIf(nameof(_isScaleChanging))] float _scaleModifier = 1.1f;
    [SerializeField, ShowIf(nameof(_isScaleChanging))] int _scaleBeats = 1;
    [SerializeField, ShowIf(nameof(_isScaleChanging))] Transform _scaleTarget = default;
    [SerializeField] bool _isAutoPlay = false;
    [SerializeField, ShowIf(nameof(_isAutoPlay))] Vector2 _delayBounds = default;
    
    
    RectTransform _fxTransform = default;

    void Awake() {
        _fxTransform = GetComponent<RectTransform>();
    }

    void Start() {
        if (_scaleTarget == null)
            _scaleTarget = transform.parent;

        if (_isAutoPlay) {
            var seq = DOTween.Sequence().SetLoops(-1, LoopType.Restart);
            seq.AppendInterval(Random.Range(_delayBounds.x, _delayBounds.y));
            seq.AppendCallback(Play);
        }
    }

    void OnEnable() {
        Reset();
    }

    public void Play() {
        Reset();
        var targetPos = _fxTransform.anchoredPosition;
        targetPos.y = _botPoint;
        _fxTransform.DOAnchorPos(targetPos, _duration).SetId(this);
        if(_isScaleChanging)
            _scaleTarget.DOScale(_scaleModifier, _duration / 2).SetLoops(2 * _scaleBeats, LoopType.Yoyo);
    }

    void Reset() {
        DOTween.Kill(this);
        var currentPos = _fxTransform.anchoredPosition;
        currentPos.y = _topPoint;
        _fxTransform.anchoredPosition = currentPos;
    }
}