using System;

using DG.Tweening;

using UnityEngine;

public class UIMiddleScreen : MonoBehaviour {
    public bool IsShowing { get; private set; } = false;

    [SerializeField] RectTransform _leftCurtain = default;
    [SerializeField] RectTransform _rightCurtain = default;
    [SerializeField] AnimationCurve _curve = default;
    
    Canvas _canvas = default;
    
    const float DURATION = 1;

    void Awake() {
        _canvas = GetComponent<Canvas>();
        _canvas.enabled = true;
        
        Switch(false, true);
    }
    
    public void Show(Action endCallback = null) {
        Switch(true, false, endCallback);
    }

    public void Hide(Action endCallback = null) {
        Switch(false, false, endCallback);
    }

    [ContextMenu(nameof(Hide))]
    void DebugHide() {
        Switch(false, false);
    }

    [ContextMenu(nameof(Show))]
    void DebugShow() {
        Switch(true, false);
    }

    void Switch(bool isShow, bool isFast, Action endCallback = null) {
        IsShowing = isShow;
        _leftCurtain.DOKill();
        _rightCurtain.DOKill();
        var leftSizeDelta = new Vector3(isShow ? 1 : 0, 1, 1);
        var rightSizeDelta = new Vector3(isShow ? 1 : 0, 1, 1);
        
        if (isFast) {
            _leftCurtain.localScale = leftSizeDelta; 
            _rightCurtain.localScale = rightSizeDelta;
            endCallback?.Invoke();
            return;
        }

        _leftCurtain.DOScale(leftSizeDelta, DURATION).SetEase(_curve);
        _rightCurtain.DOScale(rightSizeDelta, DURATION).SetEase(_curve).OnComplete(() => {endCallback?.Invoke();});
    }
}
