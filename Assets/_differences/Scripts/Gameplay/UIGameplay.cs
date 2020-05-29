using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using static UnityEngine.Mathf;

public class UIGameplay : MonoBehaviour {
    [SerializeField] Image _image1 = default;
    [SerializeField] Image _image2 = default;
    [SerializeField] TextMeshProUGUI _pointsCountText = default;
    [SerializeField] GameObject _diffVisualPrefab = default;
    
    [Inject] Database _database = default;
    [Inject] ImagesLoader _loader = default;
    
    readonly List<Point> _points = new List<Point>();

    Data _data;
    int _pointsCount;
    int _currentPointsFound = 0;

    void Start() {
        var levelData = _database.GetLevelByNum(0);
        _data = levelData;
        _loader.LoadImagesAndCreateSprite(levelData.Image1Path, levelData.Image2Path, OnLoaded);
    }
    
    void OnLoaded(Sprite im1, Sprite im2) {
        Fill(im1, im2, _data);
    }
    
    void Fill(Sprite image1, Sprite image2, Data levelData) {
        _image1.sprite = image1;
        _image2.sprite = image2;
        _pointsCount = levelData.Points.Length;
        _currentPointsFound = 0;
        _points.Clear();
        _points.AddRange(levelData.Points);
        UpdatePointsAmountVisual();
    }

    void UpdatePointsAmountVisual() {
        _pointsCountText.text = $"{_currentPointsFound}/{_pointsCount}";
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            var mousePos = Input.mousePosition;
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
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    bool IsPixelInsidePoint(Vector2 pixel, Point point) {
        // TODO: Fix it
        return true;
        // return Pow(pixel.x - point.X, 2) +
        //        Pow(pixel.y - point.Y, 2) <
        //        Pow(point.Radius, 2);
    }

    void SelectDifference(Point point) {
        _currentPointsFound++;
        UpdatePointsAmountVisual();
        CreateDiffsVisual(point);
    }
    
    void CreateDiffsVisual(Point point) {
        var handler = Instantiate(_diffVisualPrefab);
        var handlerRect = handler.GetComponent<RectTransform>();
        var pos = DiffUtils.GetRectSpaceCoordinateFromPixel(point.Center, _image1,
            _image1.GetComponent<RectTransform>());
        handlerRect.SetParent(_image1.transform, false);
        // TODO:
        // handlerRect.sizeDelta = new Vector2(point.Radius, point.Radius);
        handlerRect.localPosition = pos;
        
        var handler2 = Instantiate(_diffVisualPrefab);
        var handlerRect2 = handler2.GetComponent<RectTransform>();
        var pos2 = DiffUtils.GetRectSpaceCoordinateFromPixel(point.Center, _image2,
            _image2.GetComponent<RectTransform>());
        handlerRect2.SetParent(_image2.transform, false);
        // TODO:

        // handlerRect2.sizeDelta = new Vector2(point.Radius, point.Radius);
        handlerRect2.localPosition = pos2;
    }
}
