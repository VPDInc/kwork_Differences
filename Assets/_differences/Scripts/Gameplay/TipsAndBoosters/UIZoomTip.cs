using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIZoomTip : Tip {
    [SerializeField] float _scaleIncreaseFactor = 1.5f;
    
    [Inject] UIGameplay _gameplay = default;

    bool _isUnderZoom = false;
    Vector3 _startPos;

    protected override void Start() {
        base.Start();
        _gameplay.PointOpened += OnPointOpened;
        _gameplay.Initialized += OnInitialized;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        _gameplay.PointOpened -= OnPointOpened;
        _gameplay.Initialized -= OnInitialized;
    }

    void Update() {
        if (!_isUnderZoom)
            return;

        if (Input.GetMouseButtonDown(0)) {
            _startPos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0)) {
            var delta = Input.mousePosition - _startPos;
            var images = _gameplay.CurrentImages;
            Move(images.Item1, delta);
            Move(images.Item2, delta);
            _startPos = Input.mousePosition;
        }
    }

    void Move(Image image, Vector3 delta) {
        var rect = image.GetComponent<RectTransform>();
        var pos = rect.anchoredPosition;

        var widthLimit = rect.rect.width * (_scaleIncreaseFactor - 1) * 0.5f;
        var heightLimit = rect.rect.height * (_scaleIncreaseFactor - 1) * 0.5f;
        
        pos += new Vector2(delta.x, delta.y);
        pos.x = Mathf.Clamp(pos.x, -widthLimit, widthLimit);
        pos.y = Mathf.Clamp(pos.y, -heightLimit, heightLimit);

        rect.anchoredPosition = pos;
    }

    protected override bool TryActivate() {
        ZoomIn();
        return true;
    }
        
    void OnInitialized() {
        ZoomOut();
    }

    void OnPointOpened(Point point) {
        ZoomOut();
    }
    
    void ZoomIn() {
        var images = _gameplay.CurrentImages;
        images.Item1.transform.localScale = Vector3.one * _scaleIncreaseFactor; 
        images.Item2.transform.localScale = Vector3.one * _scaleIncreaseFactor;
        _isUnderZoom = true;
    }

    void ZoomOut() {
        var images = _gameplay.CurrentImages;
        images.Item1.transform.localScale = Vector3.one;
        images.Item2.transform.localScale = Vector3.one;
        images.Item1.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        images.Item2.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        _isUnderZoom = false;
    }
}
