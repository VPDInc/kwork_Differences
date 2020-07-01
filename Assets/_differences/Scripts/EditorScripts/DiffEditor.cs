using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Airion.Extensions;

using Sirenix.OdinInspector;
using Sirenix.Utilities;

using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using System.Collections;

using UnityEditor;

public class DiffEditor : MonoBehaviour {
    [SerializeField, ReadOnly, ShowIf(nameof(IsPlaymode))] 
    Orientation _currentOrientation = Orientation.Horizontal;
    
    Vector2 _offset;
    bool IsSelected => _currentSelectedHandler != null;
    bool IsPlaymode => Application.isPlaying;
    EditorConfig _config;
    int _currentHandlerId = 0;
    DiffHandler _currentSelectedHandler;
    readonly List<DiffHandler> _handlers = new List<DiffHandler>();


    [ShowInInspector, ShowIf(nameof(IsSelected)), PropertyRange(0, 2000)]
    float Width {
        set {
            if (_currentSelectedHandler == null)
                return;

            SetWidth(_currentSelectedHandler.Id, value);
        }
        get => _currentSelectedHandler?.Width ?? 0;
    }
    
    [ShowInInspector, ShowIf(nameof(IsSelected)), PropertyRange(0, 2000)]
    float Height {
        set {
            if (_currentSelectedHandler == null)
                return;

            SetHeight(_currentSelectedHandler.Id, value);
        }
        get => _currentSelectedHandler?.Height ?? 0;
    }
    
    [ShowInInspector, ShowIf(nameof(IsSelected)), PropertyRange(0, 179)]
    float Rotation {
        set {
            if (_currentSelectedHandler == null)
                return;

            SetRotation(_currentSelectedHandler.Id, value);
        }
        get => _currentSelectedHandler?.Rotation ?? 0;
    }

    [ShowInInspector, ShowIf(nameof(IsPlaymode))]
    Shape Shape {
        set {
            _shape = value;
            if (IsSelected)
                SetShape(_currentSelectedHandler.Id, _shape);
        }

        get => _shape;
    }

    [ShowInInspector, ShowIf(nameof(IsPlaymode))]
    Storage Storage { get; set; } = Storage.Addressable;

    Shape _shape = Shape.Rectangle;
    
    #region Unity Messages

    void Awake() {
        FindResources();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            var mousePos = Input.mousePosition;
            var hit = DiffUtils.RaycastMouse(mousePos);
            
            if (_currentSelectedHandler != null) {
                Unselect(_currentSelectedHandler.Id);
            }
            
            if (hit.gameObject != null) {
                var handler = hit.gameObject.GetComponent<DiffHandler>();

                if (handler != null) {
                    _currentSelectedHandler = handler;
                    Select(_currentSelectedHandler.Id);
                    var image = _currentSelectedHandler.transform.parent.GetComponent<Image>();
                    if (DiffUtils.GetPixelFromScreen(mousePos, image, out var imageCoords, out var localPoint)) {
                        _offset = handler.ImageSpaceCoordinates - imageCoords;
                    }

                } else {
                    var image = hit.gameObject.GetComponent<Image>();
                    if (DiffUtils.GetPixelFromScreen(mousePos, image,out var imageCoords, out var localPoint)) {
                        var imageWidth = image.sprite.texture.width;
                        var imageHeight = image.sprite.texture.height;
                        var isInWidthBounds = 0 <= imageCoords.x && imageCoords.x <= imageWidth;
                        var isInHeightBounds = 0 <= imageCoords.y && imageCoords.y <= imageHeight;

                        if (isInHeightBounds && isInWidthBounds) {
                            CreateHandler(localPoint, imageCoords, image, _currentHandlerId, Shape);
                            var images = _config.GetImages(_currentOrientation);
                            var secondImage = image == images.Item1 ? images.Item2 : images.Item1;
                            var secondLocalPoint = DiffUtils.GetRectSpaceCoordinateFromPixel(imageCoords, secondImage,
                                secondImage.GetComponent<RectTransform>());

                            CreateHandler(secondLocalPoint, imageCoords, secondImage, _currentHandlerId, Shape);
                            _currentHandlerId++;
                            
                            UpdateHandlersNum();
                        }
                    }
                }
            }
        } else if (Input.GetMouseButton(0)) {
            if (_currentSelectedHandler != null) {
                var mousePos = Input.mousePosition;
                var image = _currentSelectedHandler.transform.parent.GetComponent<Image>();
                if (DiffUtils.GetPixelFromScreen(mousePos, image, out var imageCoords, out var localPoint)) {
                    var handlers = _handlers.Where(h => h.Id == _currentSelectedHandler.Id);
                    foreach (var handler in handlers) {
                        var img = handler.transform.parent.GetComponent<Image>();
                        var imageRect = img.GetComponent<RectTransform>();
                        var pos = DiffUtils.GetRectSpaceCoordinateFromPixel(imageCoords + _offset, img, imageRect);
                        handler.GetComponent<RectTransform>().localPosition = pos;
                        handler.ImageSpaceCoordinates = imageCoords + _offset;
                    }
                }
            }
        }
    }
    
    #endregion // Unity Messages

    
    void SetWidth(int id, float value) {
        var handlers = _handlers.Where(h => h.Id == id);
        handlers.ForEach(h => h.SetWidth(value));
    }
    
    void SetHeight(int id, float value) {
        var handlers = _handlers.Where(handler => handler.Id == id);
        handlers.ForEach(h => h.SetHeight(value));
    }
    
    void SetShape(int id, Shape shape) {
        var handlers = _handlers.Where(handler => handler.Id == id);
        handlers.ForEach(h => h.SetShape(shape));
    }
    
    void SetRotation(int id, float value) {
        var handlers = _handlers.Where(handler => handler.Id == id);
        handlers.ForEach(h => h.SetRotation(value));
    }
    
    [Button, ShowIf(nameof(IsSelected))]
    void Delete() {
        var id = _currentSelectedHandler.Id;
        var toDelete = _handlers.Where(h => h.Id == id).ToArray();
        
        for (int i = 0; i < toDelete.Length; i++) {
            var handler = toDelete[i];
            _handlers.Remove(handler);
            Destroy(handler.gameObject);
        }
        
        UpdateHandlersNum();
    }
    
    [Button, ShowIf(nameof(IsPlaymode))]
    void SwitchOrientation() {
        _currentOrientation = _currentOrientation == Orientation.Horizontal
            ? Orientation.Vertical
            : Orientation.Horizontal;
        SwitchImages();
        TranslateMarks();
    }

    void SetOrientation(Orientation orientation) {
        if (_currentOrientation != orientation)
            SwitchOrientation();
    }

    void SwitchImages() {
        var newImages = _config.GetImages(_currentOrientation);
        var oldImages = _config.GetImages(_currentOrientation == Orientation.Horizontal
            ? Orientation.Vertical
            : Orientation.Horizontal);
        
        oldImages.Item1.transform.parent.gameObject.SetActive(false);
        newImages.Item1.transform.parent.gameObject.SetActive(true);

        newImages.Item1.sprite = oldImages.Item1.sprite;
        newImages.Item2.sprite = oldImages.Item2.sprite;
    }

    void TranslateMarks() {
        var newImages = _config.GetImages(_currentOrientation);
        var oldImages = _config.GetImages(_currentOrientation == Orientation.Horizontal
            ? Orientation.Vertical
            : Orientation.Horizontal);
        
        foreach (var handler in _handlers) {
            if (handler.transform.parent == oldImages.Item1.transform)
                handler.transform.SetParent(newImages.Item1.transform, false);
            else
                handler.transform.SetParent(newImages.Item2.transform, false);

            var pixels = handler.ImageSpaceCoordinates;
            var image = handler.transform.parent.GetComponent<Image>();
            var rect = image.GetComponent<RectTransform>();
            var local = DiffUtils.GetRectSpaceCoordinateFromPixel(pixels, image, rect);
            var handlerRect = handler.GetComponent<RectTransform>();
            handlerRect.localPosition = local;
            handlerRect.sizeDelta = new Vector2(DiffUtils.PixelWidthToRect(handler.Width, rect, image.sprite), 
                DiffUtils.PixelHeightToRect(handler.Height, rect, image.sprite));
        }
    }
    
    void Unselect(int id) {
        var handlers = _handlers.Where(handler => handler.Id == id);
        handlers.ForEach(h => h.IsSelected = false);
        _currentSelectedHandler = null;
    }
    
    void Select(int id) {
        var handlers = _handlers.Where(handler => handler.Id == id);
        handlers.ForEach(h => h.IsSelected = true);
    }
    
    void CreateHandler(Vector2 pos, Vector2 coords, Image image, int id, Shape shape, float width = 0, float height = 0, float rotation = 0) {
        var handler = Instantiate(_config.DifHandlerPrefab, image.transform);
        var handlerRect = handler.GetComponent<RectTransform>();
        handlerRect.localPosition = pos;
        handler.ImageSpaceCoordinates = coords;
        handler.Id = id;
        _handlers.Add(handler);
        if (width > 0 && height > 0) {
           handler.SetHeight(height);
           handler.SetWidth(width);
        }
        handler.SetShape(shape);
        handler.SetRotation(rotation);
    }

    Vector2 GetRectSpaceCoordinate(RectTransform imageRect, Vector2 localPoint) {
        var normalizedPoint = Rect.PointToNormalized(imageRect.rect, localPoint);
        var rectCoord = new Vector2(imageRect.rect.width * normalizedPoint.x, imageRect.rect.height * normalizedPoint.y);
        return rectCoord;
    }

    void FindResources() {
        _config = GetComponentInChildren<EditorConfig>();
    }

    [Button, ShowIf(nameof(IsPlaymode))]
    void CreateNew() {
        Clear();
        var images = _config.GetImages(_currentOrientation);
        var spritePath = EditorUtility.OpenFilePanel("Load file", "Assets/_differences/Images", "jpg");
        spritePath = "Assets" +spritePath.Substring(Application.dataPath.Length);
        images.Item1.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        var spritePath2 = EditorUtility.OpenFilePanel("Load file", spritePath, "jpg");
        spritePath2 = "Assets" +spritePath2.Substring(Application.dataPath.Length);
        images.Item2.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath2);
    }
    
    [Button, ShowIf(nameof(IsPlaymode))]
    void LoadJson() {
        Clear();
        
        var path = EditorUtility.OpenFilePanel("Load file", "", "json");
        if (!File.Exists(path)) {
            Err(path + " Not exist!");
            return;
        }

        var jsonString = File.ReadAllText(path);
        
        var data = DiffUtils.Parse(jsonString);
        
        LoadAndCreateImages(data);
    }

    void LoadAndCreateImages(Data data) {
        SetOrientation(data.Orientation);

        StartCoroutine(DataLoading(data));
    }

    IEnumerator DataLoading(Data data) {
        var images = _config.GetImages(_currentOrientation);
        
        var loader = new Loader(this);

        yield return loader.Run(data.Image1Path, data.Image2Path, data.Storage);
            
        images.Item1.sprite = loader.Result.Item1;
        images.Item2.sprite = loader.Result.Item2;

        var width = images.Item1.sprite.texture.width;
        var height = images.Item1.sprite.texture.height;
        
        foreach (var point in data.Points) {
            CreateHandlerFromPoint(DiffUtils.FixPointRelative(point, width, height));
        }
    }

    void CreateHandlerFromPoint(Point point) {
        var images = _config.GetImages(_currentOrientation);
        
        var localPos1 =
            DiffUtils.GetRectSpaceCoordinateFromPixel(point.Center, images.Item1, images.Item1.GetComponent<RectTransform>());

        CreateHandler(localPos1, point.Center, images.Item1, _currentHandlerId, point.Shape, point.Width, point.Height, point.Rotation);

        var localPos2 =
            DiffUtils.GetRectSpaceCoordinateFromPixel(point.Center, images.Item2, images.Item2.GetComponent<RectTransform>());


        CreateHandler(localPos2, point.Center, images.Item2, _currentHandlerId, point.Shape, point.Width, point.Height, point.Rotation);

        UpdateHandlersNum();
        
        _currentHandlerId++;
    }

    [Button, ShowIf(nameof(IsPlaymode))]
    void Clear() {
        var images = _config.GetImages(_currentOrientation);
        images.Item1.sprite = null;
        images.Item2.sprite = null;
        images.Item1.transform.DestroyAllChildren();
        images.Item2.transform.DestroyAllChildren();
        _handlers.Clear();
        _currentHandlerId = 0;
    }
    
    [Button, ShowIf(nameof(IsPlaymode))]
    void ClearPoints() {
        var images = _config.GetImages(_currentOrientation);
        images.Item1.transform.DestroyAllChildren();
        images.Item2.transform.DestroyAllChildren();
        _handlers.Clear();
        _currentHandlerId = 0;
    }
    
    [Button, ShowIf(nameof(IsPlaymode))]
    void SaveJson() {
        var data = new Data();
        var points = new List<Point>();
        var uniq = _handlers.DistinctBy(handler => handler.Id);
        
        var images = _config.GetImages(_currentOrientation);
        var imageWidth = images.Item1.sprite.texture.width;
        var imageHeight = images.Item1.sprite.texture.height;
        
        foreach (var handler in uniq.ToArray()) {
            points.Add(new Point() {
                Center = new Vector2(handler.ImageSpaceCoordinates.x / imageWidth, handler.ImageSpaceCoordinates.y / imageHeight),
                Width = handler.Width / imageWidth,
                Height = handler.Height / imageHeight,
                Number = handler.Number,
                Shape = handler.Shape,
                Rotation = handler.Rotation
            });
        }

        data.Storage = Storage;
        data.Orientation = _currentOrientation;
        data.Points = points.ToArray();

        var path = EditorUtility.SaveFilePanelInProject("Save json", "Diff_", "json", "Save json");

        data.Id = Path.GetFileNameWithoutExtension(path);
        var imagesPath = string.Empty;
        switch (Storage) {
            case Storage.Resources:
                imagesPath = $"{data.Id}/";
                break;
            case Storage.Addressable:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        data.Image1Path = $"{imagesPath}{data.Id}_1";
        data.Image2Path = $"{imagesPath}{data.Id}_2";
        
        var jsonString = JsonUtility.ToJson(data);

        File.WriteAllText(path, jsonString);
        AssetDatabase.Refresh();
    }
    
    void UpdateHandlersNum() {
        var uniqHandlers = _handlers.DistinctBy(h => h.Id).ToArray();
        for (int i = 0; i < uniqHandlers.Length; i++) {
            var id = uniqHandlers[i].Id;
            var handlers = _handlers.Where(h => h.Id == id);
            handlers.ForEach(h => h.Number = i);
        }
    }

    void Err(string message) {
        Debug.LogError($"[{GetType()}] {message}");
    }
}

#endif // UNITY_EDITOR