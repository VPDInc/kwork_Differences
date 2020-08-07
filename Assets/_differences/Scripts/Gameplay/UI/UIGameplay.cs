using System;
using System.Collections.Generic;
using System.Linq;

using Airion.Extensions;

using DG.Tweening;

using Doozy.Engine.UI;

using UnityEngine;
using UnityEngine.UI;

public class UIGameplay : MonoBehaviour {
    public event Action Initialized;
    public event Action<Point> PointOpened; 
    public (Image, Image) CurrentImages => _currentImages;
    public Point[] ClosedPoints => _closedPoints.ToArray();
    
    [SerializeField] Image _image1Hor = default;
    [SerializeField] Image _image2Hor = default;
    [SerializeField] Image _image1Vert = default;
    [SerializeField] Image _image2Vert = default;
    [SerializeField] GameObject _rectDiffVisual = default;
    [SerializeField] GameObject _circleDiffVisual = default;
    [SerializeField] UIPointsBar _helper = default;
    [SerializeField] UIView _mainView = default;
    [SerializeField] UIView _loadingView = default;
    [SerializeField] CanvasGroup _horizontalGroup = default;
    [SerializeField] CanvasGroup _verticalGroup = default;

    Data _data;
    (Image, Image) _currentImages;
    
    readonly List<Point> _closedPoints = new List<Point>();
    readonly List<Point> _allPoints = new List<Point>();

    public void Initialize(Data levelData, (Sprite, Sprite) sprites) {
        _data = levelData;
        _closedPoints.Clear();
        _allPoints.Clear();
        _allPoints.AddRange(_data.Points);
        _closedPoints.AddRange(_data.Points);
        _helper.SetPointsAmount(_allPoints.Count);

        var image1 = sprites.Item1;
        var image2 = sprites.Item2;
        var curr = _horizontalGroup;
        if (levelData.Orientation == Orientation.Vertical)
            curr = _verticalGroup;

        var seq = DOTween.Sequence();
        seq.Append(_verticalGroup.DOFade(0, 0.5f));
        seq.Join(_horizontalGroup.DOFade(0, 0.5f));
        seq.AppendCallback(() => {
            _currentImages = (_image1Hor, _image2Hor);
            if (levelData.Orientation == Orientation.Vertical) {
                _currentImages = (_image1Vert, _image2Vert);
            }
            _currentImages.Item1.sprite = image1;
            _currentImages.Item2.sprite = image2;
        });
        seq.Append(curr.DOFade(1, 0.5f));
        seq.AppendCallback(() => Initialized?.Invoke());
    }
    
    public void ShowWaitWindow() {
        _loadingView.Show();
    }

    public void HideWaitWindow() {
        _loadingView.Hide();
    }

    public void Complete() {
        _mainView.Hide();
    }

    public void Show() {
        _mainView.Show();
    }
    
    public void Clear() {
        _image1Hor.transform.DestroyAllChildren();
        _image2Hor.transform.DestroyAllChildren();
        _image1Vert.transform.DestroyAllChildren();
        _image2Vert.transform.DestroyAllChildren();
    }

    public enum OverlapStatus {
        Found,
        Doubled,
        NotFound
    }
    
    public OverlapStatus TryOverlap(Vector2 mousePos, out Point outPoint) {
        var raycast = DiffUtils.RaycastMouse(mousePos);
        if (raycast.gameObject != null) {
            var image = raycast.gameObject.GetComponent<Image>();
            if (image != null) {
                if (DiffUtils.GetPixelFromScreen(mousePos, image, out var pixelPos, out var localPos)) {
                    for (var index = 0; index < _allPoints.Count; index++) {
                        var point = _allPoints[index];
                        if (IsPixelInsidePoint(pixelPos, point)) {
                            if (_closedPoints.Any(p => p.Number == point.Number)) {
                                SelectDifference(point);
                                _closedPoints.Remove(point);
                                outPoint = point;
                                PointOpened?.Invoke(point);
                                return OverlapStatus.Found;
                            } else {
                                outPoint = default;
                                return OverlapStatus.Doubled;
                            }
                        }
                    }
                }
            }
        }

        outPoint = default;
        return OverlapStatus.NotFound;
    }
    
    public bool IsOverImage(Vector3 mousePos) {
        var raycast = DiffUtils.RaycastMouse(mousePos);
        if (raycast.gameObject != null) {
            var images = _currentImages;
            if (raycast.gameObject.Equals(images.Item1.gameObject))
                return true;
            if (raycast.gameObject.Equals(images.Item2.gameObject))
                return true;
        }

        return false;
    }

    bool IsPixelInsidePoint(Vector2 pixel, Point point) {
        switch (point.Shape) {
            case Shape.Rectangle:
                return TestRectangle(pixel, point);
            case Shape.Circle:
                return TestEllipse(pixel, point.Center, point.Width, point.Height, point.Rotation);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    bool TestRectangle(Vector2 pixel, Point point) {
        var rect = new Rect(Vector2.zero, new Vector2(point.Width, point.Height));
        rect.center = point.Center;
        return Contains(rect, point.Rotation, pixel);
    }

    bool Contains(Rect rect, float rectAngle, Vector2 point) {
        // rotate around rectangle center by -rectAngle
        var s = Mathf.Sin(-rectAngle * Mathf.Deg2Rad);
        var c = Mathf.Cos(-rectAngle * Mathf.Deg2Rad);

        // set origin to rect center
        var newPoint = point - rect.center;
        // rotate
        newPoint = new Vector2(newPoint.x * c - newPoint.y * s, newPoint.x * s + newPoint.y * c);
        // put origin back
        newPoint = newPoint + rect.center;

        // check if our transformed point is in the rectangle, which is no longer
        // rotated relative to the point

        return newPoint.x >= rect.xMin && newPoint.x <= rect.xMax && newPoint.y >= rect.yMin && newPoint.y <= rect.yMax;
    }

    bool TestEllipse(Vector2 point, Vector2 center, float width, float height, float angle) {
        // #tests if a point[xp,yp] is within
        // #boundaries defined by the ellipse
        // #of center[x,y], diameter d D, and tilted at angle
        var cosa = Mathf.Cos(angle * Mathf.Deg2Rad);
        var sina = Mathf.Sin(angle * Mathf.Deg2Rad);
        var widthD = (width * 0.5f) * (width * 0.5f);
        var heightD = (height * 0.5f) * (height * 0.5f);

        var a = Mathf.Pow(cosa * (point.x - center.x) + sina * (point.y - center.y), 2);
        var b = Mathf.Pow(sina * (point.x - center.x) - cosa * (point.y - center.y), 2);
        var ellipse = (a / widthD) + (b / heightD);

        return ellipse <= 1;
    }

    void SelectDifference(Point point) {
        var orderedPoints = _data.Points.OrderByDescending(p => p.Height * p.Width).ToArray();
        for (int i = 0; i < orderedPoints.Length; i++) {
            if (point.Number == orderedPoints[i].Number)
                _helper.Open(i);
        }
        
        CreateDiffsVisual(point);
    }
    
    void CreateDiffsVisual(Point point) {
        GameObject diffPrefab = null;
        switch (point.Shape) {
            case Shape.Rectangle:
                diffPrefab = _rectDiffVisual;
                break;
            case Shape.Circle:
                diffPrefab = _circleDiffVisual;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        var handler = Instantiate(diffPrefab);
        var handlerRect = handler.GetComponent<RectTransform>();
        var image1Rect = _currentImages.Item1.GetComponent<RectTransform>();
        var pos = DiffUtils.GetRectSpaceCoordinateFromPixel(point.Center, _currentImages.Item1, image1Rect);
        handlerRect.SetParent(_currentImages.Item1.transform, false);
        handlerRect.sizeDelta = new Vector2(DiffUtils.PixelWidthToRect(point.Width, image1Rect, _currentImages.Item1.sprite), 
            DiffUtils.PixelHeightToRect(point.Height, image1Rect, _currentImages.Item1.sprite));
        handlerRect.localPosition = pos;
        handler.transform.rotation = Quaternion.Euler(0,0, point.Rotation);
        
        var handler2 = Instantiate(diffPrefab);
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
}
