using System;

using DG.Tweening;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIZoomTip : Tip {
    [SerializeField] float _scaleIncreaseFactor = 1.5f;
    [SerializeField] float _sensitivity = 0.1f;
    [SerializeField] float _durationSec = 20;
    [SerializeField] TextMeshProUGUI _timeText = default;
    [SerializeField] GameObject _timeGroup = default;
    
    [Inject] UIGameplay _gameplay = default;
    [Inject] UITimer _timer = default;

    bool _isUnderZoom = false;
    Vector3 _startPos;
    float _startDistance = 0;
    float _currentZoom = 0;
    float _startTimestamp = 0;
    float _last = 0;

    protected override void Start() {
        base.Start();

        _timer.Started += OnTimerStarted;
        _timer.Stopped += OnTimerStopped;
        _gameplay.Initialized += OnInitialized;
        _gameplayHandler.GameEnded += OnGameEnded;
        _gameplayHandler.PictureChangingStarted += OnPictureChanged;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        
        _timer.Started -= OnTimerStarted;
        _timer.Stopped -= OnTimerStopped;
        _gameplay.Initialized -= OnInitialized;
        _gameplayHandler.GameEnded -= OnGameEnded;
        _gameplayHandler.PictureChangingStarted -= OnPictureChanged;
    }
    
    void OnTimerStarted() {
        if (_last > 0) {
            _startTimestamp = Time.time - (_durationSec - _last);
            _isUnderZoom = true;
            _last = 0;
        }
    }
    
    void OnTimerStopped() {
        if (_isUnderZoom) {
            _last = _durationSec - (Time.time - _startTimestamp);
            _isUnderZoom = false;
            return;
        }

        _last = 0;
    }
    
    void OnPictureChanged() {
        if (_isUnderZoom)
            ZoomOut();
    }

    void OnGameEnded() {
        if (_isUnderZoom)
            ZoomOut();
    }

    void Update() {
        if (!_isUnderZoom)
            return;

        UpdateTimer();

        if (Time.time - _startTimestamp >= _durationSec) {
            ZoomOut();
        }
        
        if (Input.touchCount == 1)
            CheckMove();

        if (Input.touchCount >= 2)
            CheckZoom();
    }

    void UpdateTimer() {
        _timeText.text = Mathf.Max(_durationSec - (Time.time - _startTimestamp), 0).ToString("F0");
    }

    void CheckMove() {
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

    void CheckZoom() {
        var touch1 = Input.GetTouch(0);
        var touch2 = Input.GetTouch(1);

        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began) {
            _startDistance = Vector3.Distance(touch1.position, touch2.position);
        } else {
            var currentDistance = Vector2.Distance(touch1.position, touch2.position);
            var distanceDelta = currentDistance - _startDistance;
            _startDistance = currentDistance;
            Zoom(distanceDelta * _sensitivity);
        }

        if (Input.GetKey(KeyCode.Alpha1)) {
            Zoom(-0.01f);
        }
        
        if (Input.GetKey(KeyCode.Alpha2)) {
            Zoom(0.01f);
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
        if (_isUnderZoom)
            return false;
        
        ZoomIn();
        return true;
    }
        
    void OnInitialized() {
        ZoomOut(true);
    }

    void ZoomIn() {
        DOTween.Kill(this);
        
        var images = _gameplay.CurrentImages;

        images.Item1.transform.DOScale(_scaleIncreaseFactor, 1).SetId(this);
        images.Item2.transform.DOScale(_scaleIncreaseFactor, 1).SetId(this);
        _currentZoom = _scaleIncreaseFactor;
        _startTimestamp = Time.time;
        UpdateTimer();
        _timeGroup.SetActive(true);
        _isUnderZoom = true;
    }

    void ZoomOut(bool fast = false) {
        DOTween.Kill(this);

        var images = _gameplay.CurrentImages;
        
        if (fast) {
            images.Item1.transform.localScale = Vector3.one;
            images.Item2.transform.localScale = Vector3.one;
            images.Item1.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            images.Item2.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        } else {
            images.Item1.transform.DOScale(1, 1).SetId(this);
            images.Item2.transform.DOScale(1, 1).SetId(this);
            images.Item1.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 1).SetId(this);
            images.Item2.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 1).SetId(this);
        }
        
        _timeGroup.SetActive(false);
        _currentZoom = 0;
        _isUnderZoom = false;
    }
}
