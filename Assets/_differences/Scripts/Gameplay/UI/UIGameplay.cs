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
    public (Image, Image) CurrentImages => _currentSetter.GetImages();
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
    [SerializeField] ImageSetter _horizontal = default;
    [SerializeField] ImageSetter _vertical = default;
    [SerializeField] float _touchRadius = 100f;

    Data _data;
    ImageSetter _currentSetter;
    
    readonly List<Point> _closedPoints = new List<Point>();
    readonly List<Point> _allPoints = new List<Point>();

    public void SwitchData(Data levelData, (Sprite, Sprite) sprites) {
        _data = levelData;
        _data.Points = FixPoints(_data.Points, (sprites.Item1, sprites.Item2));
        _closedPoints.Clear();
        _allPoints.Clear();
        _allPoints.AddRange(_data.Points);
        _closedPoints.AddRange(_data.Points);
        _helper.SetPointsAmount(_allPoints.Count);

        var image1 = sprites.Item1;
        var image2 = sprites.Item2;

        var curr = _horizontal;
        if (levelData.Orientation == Orientation.Vertical)
            curr = _vertical;

        var seq = DOTween.Sequence();
        seq.AppendCallback(()=>_currentSetter.Hide());
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() => {
            _currentSetter = levelData.Orientation == Orientation.Vertical ? _vertical : _horizontal;
            _currentSetter.Set(image1, image2);
        });
        seq.AppendCallback(()=>curr.Show());
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() => Initialized?.Invoke());
    }

    public void StartWithData(Data[] levelData, (Sprite, Sprite)[] sprites, Action callback) {
        _data = levelData[0];
        _data.Points = FixPoints(_data.Points, (sprites[0].Item1, sprites[0].Item2));
        _closedPoints.Clear();
        _allPoints.Clear();
        _allPoints.AddRange(_data.Points);
        _closedPoints.AddRange(_data.Points);
        _helper.SetPointsAmount(_allPoints.Count);
        
        _horizontal.Hide(true);
        _vertical.Hide(true);
        _currentSetter = _data.Orientation == Orientation.Vertical ? _vertical : _horizontal;

        var setters = new List<ImageSetter>();

        var startPos = _currentSetter.Rect.anchoredPosition.x;
        var offset = _currentSetter.Rect.rect.width;
        
        var scaleMult = 0.95f;
        var scaleMovingDuration = 0.2f;
        var movingDuration = 0.5f;
        
        for (int i = levelData.Length - 1; i >= 0; i--) {
            var data = levelData[i];
            ImageSetter setter;
            if (i == 0) {
                setter = _currentSetter;
            } else {
                var select = data.Orientation == Orientation.Vertical ? _vertical : _horizontal;
                setter = Instantiate(select, select.transform.parent);
            }

            setter.Set(sprites[i].Item1, sprites[i].Item2);
            setter.Show(true);
            setters.Add(setter);
            if (i < levelData.Length - 1) {
                setter.Rect.DOAnchorPosX(startPos - offset, 0);
                setter.transform.DOScale(scaleMult, 0);
            }
        }

        var seq = DOTween.Sequence();
        seq.AppendInterval(1.5f);
        if (setters.Count > 1) {
            for (int i = 0; i < setters.Count - 1; i++) {
                var index = i;
                var obj = setters[index];
                var nextObj = setters[index + 1];
                seq.Append(obj.transform.DOScale(scaleMult, scaleMovingDuration));
                seq.Append(obj.Rect.DOAnchorPosX(startPos + offset, movingDuration).SetEase(Ease.Linear)).OnComplete(()=>Destroy(obj.gameObject));
                seq.Join(nextObj.Rect.DOAnchorPosX(startPos, movingDuration).SetEase(Ease.Linear));
                seq.Append(nextObj.transform.DOScale(1, scaleMovingDuration));
                seq.AppendInterval(0.3f);
            }
        }

        seq.AppendCallback(() => {
            Initialized?.Invoke();
            callback?.Invoke();
        });

    }
    
    Point[] FixPoints(Point[] points, (Sprite, Sprite) loadedSprite) {
        var fixedPoints = new Point[points.Length];
        for (int i = 0; i < points.Length; i++) {
            fixedPoints[i] = DiffUtils.FixPointRelative(points[i], loadedSprite.Item1);
        }

        return fixedPoints;
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
                    var overlappedPoints = new List<Point>();
                    for (var index = 0; index < _allPoints.Count; index++) {
                        var point = _allPoints[index];
                        if (IsPixelInsidePoint(pixelPos, point)) {
                            overlappedPoints.Add(point);
                        }
                    }

                    if (overlappedPoints.Count > 0) {
                        var ordered = overlappedPoints.OrderBy(p => Vector2.Distance(p.Center, pixelPos));
                        var nearest = ordered.First();
                        if (_closedPoints.Any(p => p.Number == nearest.Number)) {
                            SelectDifference(nearest);
                            _closedPoints.Remove(nearest);
                            outPoint = nearest;
                            PointOpened?.Invoke(nearest);
                            return OverlapStatus.Found;
                        } else {
                            outPoint = default;
                            return OverlapStatus.Doubled;
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
            var images = _currentSetter.GetImages();

            if (raycast.gameObject.Equals(images.Item1.gameObject))
                return true;
            if (raycast.gameObject.Equals(images.Item2.gameObject))
                return true;
        }

        return false;
    }

    bool IsPixelInsidePoint(Vector2 pixel, Point point) {
        return CollisionUtils.TestCircleRect(pixel, _touchRadius, point.Center, point.Width, point.Height, point.Rotation);
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
        
        var images = _currentSetter.GetImages();
            
       CreateDiff(diffPrefab, images.Item1, point);
       CreateDiff(diffPrefab, images.Item2, point);
    }

    void CreateDiff(GameObject diffPrefab, Image image, Point point) {
        var handler = Instantiate(diffPrefab);
        var handlerRect = handler.GetComponent<RectTransform>();
        var imageRect = image.GetComponent<RectTransform>();
        var pos = DiffUtils.GetRectSpaceCoordinateFromPixel(point.Center, image, imageRect);
        handlerRect.SetParent(image.transform, false);
        handlerRect.sizeDelta = new Vector2(DiffUtils.PixelWidthToRect(point.Width, imageRect, image.sprite), 
            DiffUtils.PixelHeightToRect(point.Height, imageRect, image.sprite));
        handlerRect.localPosition = pos;
        handler.transform.rotation = Quaternion.Euler(0,0, point.Rotation);
    }
}
