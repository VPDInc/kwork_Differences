using System;
using System.Collections.Generic;

using Airion.Extensions;

using Doozy.Engine.UI;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIGameplay : MonoBehaviour {
    public event Action Initialized;
    public event Action<Point> PointOpened; 
    public (Image, Image) CurrentImages => _currentImages;
    public Point[] ClosedPoints => _points.ToArray();
    
    [SerializeField] Image _image1Hor = default;
    [SerializeField] Image _image2Hor = default;
    [SerializeField] Image _image1Vert = default;
    [SerializeField] Image _image2Vert = default;
    [SerializeField] GameObject _diffVisualPrefab = default;
    [SerializeField] UIPointsBar _helper = default;
    [SerializeField] UIView _view = default;
    
    [Inject] ImagesLoader _loader = default;
    
    readonly List<Point> _points = new List<Point>();

    Data _data;
    (Image, Image) _currentImages;

    public void Initialize(Data levelData) {
        _data = levelData;
        // TODO: Move in to GameplayHandler
        _loader.LoadImagesAndCreateSprite(levelData.Image1Path, levelData.Image2Path, OnLoaded);
    }

    public void Complete() {
        _view.Hide();
    }
    
    public void Clear() {
        _image1Hor.transform.DestroyAllChildren();
        _image2Hor.transform.DestroyAllChildren();
        _image1Vert.transform.DestroyAllChildren();
        _image2Vert.transform.DestroyAllChildren();
    }
    
    public bool IsOverlap(Vector2 mousePos, out Point outPoint) {
        var raycast = DiffUtils.RaycastMouse(mousePos);
        if (raycast.gameObject != null) {
            var image = raycast.gameObject.GetComponent<Image>();
            if (image != null) {
                if (DiffUtils.GetPixelFromScreen(mousePos, image, out var pixelPos, out var localPos)) {
                    for (var index = 0; index < _points.Count; index++) {
                        var point = _points[index];
                        if (IsPixelInsidePoint(pixelPos, point)) {
                            SelectDifference(point);
                            _points.RemoveAt(index);
                            outPoint = point;
                            PointOpened?.Invoke(point);
                            return true;
                        }
                    }
                }
            }
        }

        outPoint = default;
        return false;
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

    void OnLoaded(Sprite im1, Sprite im2) {
        Fill(im1, im2, _data);
        _points.Clear();
        _points.AddRange(_data.Points);
        _helper.SetPointsAmount(_points.Count);
        Initialized?.Invoke();
    }
    
    void Fill(Sprite image1, Sprite image2, Data levelData) {
        _image1Hor.transform.parent.parent.gameObject.SetActive(false);
        _image1Vert.transform.parent.parent.gameObject.SetActive(false);
        
        _currentImages = (_image1Hor, _image2Hor);
        if (levelData.Orientation == Orientation.Vertical)
            _currentImages = (_image1Vert, _image2Vert);
        
        _currentImages.Item1.transform.parent.parent.gameObject.SetActive(true);

        _currentImages.Item1.sprite = image1;
        _currentImages.Item2.sprite = image2;
        
        _view.Show();
    }

    bool IsPixelInsidePoint(Vector2 pixel, Point point) {
        var rect = new Rect(Vector2.zero, new Vector2(point.Width, point.Height));
        rect.center = point.Center;
        return rect.Contains(pixel);
    }

    void SelectDifference(Point point) {
        _helper.Open(point.Number);
        CreateDiffsVisual(point);
    }
    
    void CreateDiffsVisual(Point point) {
        var handler = Instantiate(_diffVisualPrefab);
        var handlerRect = handler.GetComponent<RectTransform>();
        var image1Rect = _currentImages.Item1.GetComponent<RectTransform>();
        var pos = DiffUtils.GetRectSpaceCoordinateFromPixel(point.Center, _currentImages.Item1, image1Rect);
        handlerRect.SetParent(_currentImages.Item1.transform, false);
        handlerRect.sizeDelta = new Vector2(DiffUtils.PixelWidthToRect(point.Width, image1Rect, _currentImages.Item1.sprite), 
            DiffUtils.PixelHeightToRect(point.Height, image1Rect, _currentImages.Item1.sprite));
        handlerRect.localPosition = pos;
        
        var handler2 = Instantiate(_diffVisualPrefab);
        var handlerRect2 = handler2.GetComponent<RectTransform>();
        var pos2 = DiffUtils.GetRectSpaceCoordinateFromPixel(point.Center, _currentImages.Item2,
            _currentImages.Item2.GetComponent<RectTransform>());
        handlerRect2.SetParent(_currentImages.Item2.transform, false);
        var image2Rect = _currentImages.Item1.GetComponent<RectTransform>();
        handlerRect2.sizeDelta = new Vector2(DiffUtils.PixelWidthToRect(point.Width, image2Rect, _currentImages.Item2.sprite), 
            DiffUtils.PixelHeightToRect(point.Height, image2Rect, _currentImages.Item2.sprite));
        handlerRect2.localPosition = pos2;
    }
}
