using System.Collections.Generic;
using System.Linq;

using Airion.Extensions;

using DG.Tweening;

using Sirenix.Utilities;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIAimTip : Tip {
    [SerializeField] AimVisual _aimPrefab = default;
    [SerializeField] UITrailEffect _effectTrailEffect = default;

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
    
    protected override bool TryActivate() {
        var notOpenedPointsSet = _notOpenedPoints.ToHashSet();
        var lastPoints = _gameplay.ClosedPoints.ToHashSet();
        notOpenedPointsSet.IntersectWith(lastPoints);
        if (notOpenedPointsSet.Count == 0)
            return false;

        ShowTip();
        return true;
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
        CreateAim(point, _currentImages.Item1);
        CreateAim(point, _currentImages.Item2);
    }

    void CreateAim(Point point, Image image) {
        var effect = Instantiate(_effectTrailEffect, transform);
        var handler = Instantiate(_aimPrefab);
        handler.Id = point.Number;
        _aims.Add(handler);
        var handlerRect = handler.GetComponent<RectTransform>();
        var imageRect = image.GetComponent<RectTransform>();
        var pos = DiffUtils.GetRectSpaceCoordinateFromPixel(point.Center, image, imageRect);
        handlerRect.SetParent(image.transform, false);
        handlerRect.sizeDelta = new Vector2(DiffUtils.PixelWidthToRect(point.Width, imageRect, image.sprite), 
                                            DiffUtils.PixelHeightToRect(point.Height, imageRect, image.sprite));
        handlerRect.localPosition = pos;
        handler.transform.rotation = Quaternion.Euler(0,0, point.Rotation);
        effect.Setup(handler.transform.position);
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
