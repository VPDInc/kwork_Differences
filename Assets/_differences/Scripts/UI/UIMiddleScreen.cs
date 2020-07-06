using DG.Tweening;

using UnityEngine;

public class UIMiddleScreen : MonoBehaviour {
    [SerializeField] RectTransform _leftCurtain = default;
    [SerializeField] RectTransform _rightCurtain = default;
    [SerializeField] AnimationCurve _curve = default;
    
    float _leftCurtainStartXSize;
    float _rightCurtainStartXSize;

    const float DURATION = 1;

    void Awake() {
        _leftCurtainStartXSize = _leftCurtain.sizeDelta.x;
        _rightCurtainStartXSize = _rightCurtain.sizeDelta.x;
        Switch(false, true);
    }
    
    [ContextMenu(nameof(Show))]
    public void Show() {
        Switch(true, false);
    }

    [ContextMenu(nameof(Hide))]
    public void Hide() {
        Switch(false, false);
    }

    void Switch(bool isShow, bool isFast) {
        _leftCurtain.DOKill();
        _rightCurtain.DOKill();
        
        if (isFast) {
            _leftCurtain.sizeDelta = new Vector2(isShow ? _leftCurtainStartXSize : 0, _leftCurtain.sizeDelta.y); 
            _rightCurtain.sizeDelta = new Vector2(isShow ? _rightCurtainStartXSize : 0, _rightCurtain.sizeDelta.y);
            return;
        }

        _leftCurtain.DOSizeDelta(new Vector2(isShow ? _leftCurtainStartXSize : 0, _leftCurtain.sizeDelta.y), DURATION).SetEase(_curve);
        _rightCurtain.DOSizeDelta(new Vector2(isShow ? _rightCurtainStartXSize : 0, _rightCurtain.sizeDelta.y), DURATION).SetEase(_curve);
    }
}
