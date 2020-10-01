using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

public class ImageSetter : MonoBehaviour {
    // public CanvasGroup Group => _group;
    public RectTransform Rect => _rect;
    
    [SerializeField] Image _image1 = default;
    [SerializeField] Image _image2 = default;
    [SerializeField] CanvasGroup _group = default;

    RectTransform _rect;

    void Awake() {
        _rect = GetComponent<RectTransform>();
    }

    public void Set(Sprite sp1, Sprite sp2) {
        _image1.sprite = sp1;
        _image2.sprite = sp2;
    }

    public (Sprite, Sprite) Get() {
        return (_image1.sprite, _image2.sprite);
    }
    
    public (Image, Image) GetImages() {
        return (_image1, _image2);
    }
    
    public void Show(bool fast = false) {
        _group.blocksRaycasts = true;

        if (fast) {
            _group.alpha = 1;
            return;
        }

        _group.DOFade(1, 0.5f);
    }

    public void Hide(bool fast = false) {
        _group.blocksRaycasts = false;
        
        if (fast) {
            _group.alpha = 0;
            return;
        }
        
        _group.DOFade(0, 0.5f);
    }
}
