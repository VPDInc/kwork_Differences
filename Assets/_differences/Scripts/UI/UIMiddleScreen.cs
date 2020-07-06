using System;

using DG.Tweening;

using UnityEngine;

public class UIMiddleScreen : MonoBehaviour {
    public bool IsShowing { get; private set; } = false;

    [SerializeField] RectTransform _leftCurtain = default;
    [SerializeField] RectTransform _rightCurtain = default;
    [SerializeField] AnimationCurve _curve = default;
    
    float _leftCurtainStartXSize;
    float _rightCurtainStartXSize;
    Canvas _canvas = default;
    
    const float DURATION = 1;

    void Awake() {
        _leftCurtainStartXSize = _leftCurtain.sizeDelta.x;
        _rightCurtainStartXSize = _rightCurtain.sizeDelta.x;
        _canvas = GetComponent<Canvas>();
        _canvas.enabled = true;
        
        Switch(false, true);
    }
    
    [ContextMenu(nameof(Show))]
    public void Show(Action endCallback = null) {
        Switch(true, false, endCallback);
    }

    [ContextMenu(nameof(Hide))]
    public void Hide(Action endCallback = null) {
        Switch(false, false, endCallback);
    }

    void Switch(bool isShow, bool isFast, Action endCallback = null) {
        IsShowing = isShow;
        _leftCurtain.DOKill();
        _rightCurtain.DOKill();
        var leftSizeDelta = new Vector2(isShow ? _leftCurtainStartXSize : 0, _leftCurtain.sizeDelta.y);
        var rightSizeDelta = new Vector2(isShow ? _rightCurtainStartXSize : 0, _rightCurtain.sizeDelta.y);
        
        if (isFast) {
            _leftCurtain.sizeDelta = leftSizeDelta; 
            _rightCurtain.sizeDelta = rightSizeDelta;
            endCallback?.Invoke();
            return;
        }

        _leftCurtain.DOSizeDelta(leftSizeDelta, DURATION).SetEase(_curve);
        _rightCurtain.DOSizeDelta(rightSizeDelta, DURATION).SetEase(_curve).OnComplete(() => {endCallback?.Invoke();});
    }
}
