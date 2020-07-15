using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIZoomTip : Tip {
    [SerializeField] float _scaleIncreaseFactor = 1.5f;
    
    [Inject] UIGameplay _gameplay = default;

    bool _isUnderZoom = false;
    Vector3 _startPos;
    float _startDistance = 0;
    float _currentZoom = 0;

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
            if (Input.touchCount >= 2) {
                _startDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
            }
        }

        if (Input.GetKey(KeyCode.Alpha1)) {
            Zoom(-0.01f);
        }
        
        if (Input.GetKey(KeyCode.Alpha2)) {
            Zoom(0.01f);
        }

        if (Input.GetMouseButton(0)) {
            var delta = Input.mousePosition - _startPos;
            var images = _gameplay.CurrentImages;

            if (Input.touchCount >= 2) {
                var currentDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                var distanceDelta = currentDistance - _startDistance;
                _startDistance = currentDistance;
                Zoom(distanceDelta);
            } else {
                Move(images.Item1, delta);
                Move(images.Item2, delta);
            }
            
            _startPos = Input.mousePosition;
        }
    }

    void Zoom(float distanceDelta) {
        var zoom = Mathf.Clamp(_currentZoom + distanceDelta, 1, _scaleIncreaseFactor);
        
        var images = _gameplay.CurrentImages;

        images.Item1.transform.localScale = Vector3.one* zoom;
        images.Item2.transform.localScale = Vector3.one* zoom;

        _currentZoom = _scaleIncreaseFactor;
        
        _currentZoom = zoom;
        
        Move(images.Item1, Vector3.zero);
        Move(images.Item2, Vector3.zero);
    }

    void Move(Image image, Vector3 delta) {
        var rect = image.GetComponent<RectTransform>();
        var pos = rect.anchoredPosition;

        var widthLimit = rect.rect.width * (_currentZoom - 1) * 0.5f;
        var heightLimit = rect.rect.height * (_currentZoom - 1) * 0.5f;
        
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
        DOTween.Kill(this);
        
        var images = _gameplay.CurrentImages;

        images.Item1.transform.DOScale(_scaleIncreaseFactor, 1).SetId(this);
        images.Item2.transform.DOScale(_scaleIncreaseFactor, 1).SetId(this);
        _currentZoom = _scaleIncreaseFactor;
        _isUnderZoom = true;
    }

    void ZoomOut() {
        DOTween.Kill(this);

        var images = _gameplay.CurrentImages;
        images.Item1.transform.DOScale(1, 1).SetId(this);
        images.Item2.transform.DOScale(1, 1).SetId(this);
        // images.Item1.transform.localScale = Vector3.one;
        // images.Item2.transform.localScale = Vector3.one;
        // images.Item1.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        // images.Item2.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        images.Item1.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 1).SetId(this);
        images.Item2.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 1).SetId(this);
        _currentZoom = 0;
        _isUnderZoom = false;
    }
}
