using System.Collections.Generic;
using System.Linq;

using Airion.Extensions;

using Sirenix.Utilities;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIAimTip : Tip {
    [SerializeField] AimVisual _aimPrefab = default;

    [Inject] UIGameplay _gameplay = default;

    (Image, Image) _currentImages;
    readonly List<Point> _notOpenedPoints = new List<Point>();

    readonly List<AimVisual> _aims = new List<AimVisual>();
    
    protected override void Start() {
        base.Start();
        _gameplay.Initialized += OnInit;
        _gameplay.PointOpened += OnPointOpened;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        _gameplay.Initialized -= OnInit;
        _gameplay.PointOpened -= OnPointOpened;
    }

    void OnInit() {
        _currentImages = _gameplay.CurrentImages;
        _notOpenedPoints.Clear();
        _notOpenedPoints.AddRange(_gameplay.ClosedPoints);
        _aims.Clear();
    }

    public override void OnButtonClick() {
        var notOpenedPointsSet = _notOpenedPoints.ToHashSet();
        var lastPoints = _gameplay.ClosedPoints.ToHashSet();
        notOpenedPointsSet.IntersectWith(lastPoints);
        if (notOpenedPointsSet.Count == 0)
            return;

        if (_currency.IsEnough(1)) {
            _currency.Spend(1);
            ShowTip();
        }
    }

    public void ShowTip() {
        var point = GetPoint();
        if (point.HasValue)
            CreateAims(point.Value);
    }

    Point? GetPoint() {
        var notOpenedPointsSet = _notOpenedPoints.ToHashSet();
        var lastPoints = _gameplay.ClosedPoints.ToHashSet();
        notOpenedPointsSet.IntersectWith(lastPoints);
        if (notOpenedPointsSet.Count == 0)
            return null;
        
        var point = notOpenedPointsSet.RandomElement();
        _notOpenedPoints.Remove(point);
        return point;
    }
    
    void CreateAims(Point point) {
        var handler = Instantiate(_aimPrefab);
        handler.Id = point.Number;
        _aims.Add(handler);
        var handlerRect = handler.GetComponent<RectTransform>();
         var image1Rect = _currentImages.Item1.GetComponent<RectTransform>();
        var pos = DiffUtils.GetRectSpaceCoordinateFromPixel(point.Center, _currentImages.Item1, image1Rect);
        handlerRect.SetParent(_currentImages.Item1.transform, false);
        handlerRect.sizeDelta = new Vector2(DiffUtils.PixelWidthToRect(point.Width, image1Rect, _currentImages.Item1.sprite), 
            DiffUtils.PixelHeightToRect(point.Height, image1Rect, _currentImages.Item1.sprite));
        handlerRect.localPosition = pos;
        handler.transform.rotation = Quaternion.Euler(0,0, point.Rotation);
        
        var handler2 = Instantiate(_aimPrefab);
        handler2.Id = point.Number;
        _aims.Add(handler2);
        var handlerRect2 = handler2.GetComponent<RectTransform>();
        var pos2 = DiffUtils.GetRectSpaceCoordinateFromPixel(point.Center, _currentImages.Item2,
            _currentImages.Item2.GetComponent<RectTransform>());
        handlerRect2.SetParent(_currentImages.Item2.transform, false);
        var image2Rect = _currentImages.Item1.GetComponent<RectTransform>();
        handlerRect2.sizeDelta = new Vector2(DiffUtils.PixelWidthToRect(point.Width, image2Rect, _currentImages.Item2.sprite), 
            DiffUtils.PixelHeightToRect(point.Height, image2Rect, _currentImages.Item2.sprite));
        handlerRect2.localPosition = pos2;
        handler2.transform.rotation = Quaternion.Euler(0,0, point.Rotation);
    }

    void OnPointOpened(Point point) {
        var handlers = _aims.Where(handler => handler.Id == point.Number).ToArray();
        for (int i = 0; i < handlers.Length; i++) {
            var h = handlers[i];
            _aims.Remove(h);
            Destroy(h.gameObject);
        }
    }
}
