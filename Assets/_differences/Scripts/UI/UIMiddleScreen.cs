using System;

using Airion.Audio;

using DG.Tweening;

using UnityEngine;

using Zenject;

public class UIMiddleScreen : MonoBehaviour {
    public bool IsShowing { get; private set; } = true;

    [SerializeField] RectTransform _leftCurtain = default;
    [SerializeField] RectTransform _rightCurtain = default;
    [SerializeField] AnimationCurve _curve = default;

    [Inject] AudioManager _audioManager = default;
    
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
        if (IsShowing == isShow) {
            endCallback?.Invoke();
            return;
        }
        
        IsShowing = isShow;
        _leftCurtain.DOKill();
        _rightCurtain.DOKill();
        var leftSizeDelta = new Vector3(isShow ? 1 : 0, 1, 1);
        var rightSizeDelta = new Vector3(isShow ? 1 : 0, 1, 1);
        var leftMovePos = isShow ? 0 : -Screen.width * 0.5f;
        var rightMovePos = isShow ? Screen.width : Screen.width + Screen.width * 0.5f;
        
        if (isFast) {
            _leftCurtain.localScale = leftSizeDelta; 
            _rightCurtain.localScale = rightSizeDelta;
            _leftCurtain.DOMoveX(leftMovePos, 0);
            _rightCurtain.DOMoveX(rightMovePos, 0);
            
            endCallback?.Invoke();
            return;
        }

        _audioManager.PlayOnce("curtains");

        _leftCurtain.DOMoveX(leftMovePos, DURATION);
        _rightCurtain.DOMoveX(rightMovePos, DURATION);
        
        _leftCurtain.DOScale(leftSizeDelta, DURATION).SetEase(_curve);
        _rightCurtain.DOScale(rightSizeDelta, DURATION).SetEase(_curve).OnComplete(() => {endCallback?.Invoke();});
    }
}
