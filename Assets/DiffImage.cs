using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Airion.Extensions;

using Sirenix.OdinInspector;
using Sirenix.Utilities;

using UnityEditor;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DiffImage : MonoBehaviour {
    [SerializeField] Sprite _image1 = default;
    [SerializeField] Sprite _image2 = default;
    
    bool IsPlaymode => Application.isPlaying;
    EditorConfig _config;
    int _currentHandlerId = 0;

    readonly List<DiffHandler> _handlers = new List<DiffHandler>();

    
    void Awake() {
        FindResources();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            var hit = RaycastMouse();
            if (hit.gameObject != null) {
                var imageRect = hit.gameObject.GetComponent<RectTransform>();
                var mousePos = Input.mousePosition;
                var image = hit.gameObject.GetComponent<Image>();
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(imageRect, mousePos, null, out var localPoint)) {
                    var imageCoords = GetPixelSpaceCoordinateFromRectPoint(localPoint, image, imageRect);
                    var imageWidth = image.sprite.texture.width;
                    var imageHeight = image.sprite.texture.height;
                    var isInWidthBounds = 0 <= imageCoords.x && imageCoords.x <= imageWidth;
                    var isInHeightBounds = 0 <= imageCoords.y && imageCoords.y <= imageHeight;
                    
                    if (isInHeightBounds && isInWidthBounds) {
                        CreateHandler(localPoint, imageCoords, image, _currentHandlerId);

                        var secondImage = image == _config.Image1 ? _config.Image2 : _config.Image1;
                        var secondLocalPoint = GetRectSpaceCoordinateFromPixel(imageCoords, secondImage,
                            secondImage.GetComponent<RectTransform>());
                        
                        CreateHandler(secondLocalPoint, imageCoords, secondImage, _currentHandlerId);
                        _currentHandlerId++;
                    }
                }
            }
        }
    }
    
    void CreateHandler(Vector2 pos, Vector2 coords, Image image, int id) {
        var handler = Instantiate(_config.DifHandlerPrefab);
        var handlerRect = handler.GetComponent<RectTransform>();
        handlerRect.SetParent(image.transform, false);
        handlerRect.localPosition = pos;
        handler.ImageSpaceCoordinates = coords;
        handler.Id = id;
        _handlers.Add(handler);
    }

    RaycastResult RaycastMouse() {
        var pointerData = new PointerEventData(EventSystem.current) {
            pointerId = -1,
        };

        pointerData.position = Input.mousePosition;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results.FirstOrDefault();
    }

    Vector2 GetPixelSpaceCoordinateFromRectPoint(Vector2 pos, Image image, RectTransform imageRect) {
        var locationRelativeToImageInScreenCoordinates = new Vector2();
        var pivotCancelledLocation =
            new Vector2(pos.x - imageRect.rect.x, pos.y - imageRect.rect.y);
        var locationRelativeToImage01 = new Vector2();
        var imageAspectRatio = image.sprite.rect.height / image.sprite.rect.width;
        var rectAspectRatio = imageRect.rect.height / imageRect.rect.width;
        var imageRectInLocalScreenCoordinates = new Rect();
        if (imageAspectRatio > rectAspectRatio) {
            // The image is constrained by its height.
            var imageWidth = (rectAspectRatio / imageAspectRatio) * imageRect.rect.width;
            var excessWidth = imageRect.rect.width - imageWidth;
            imageRectInLocalScreenCoordinates.Set(imageRect.pivot.x * excessWidth, 0,
                imageRect.rect.height / imageAspectRatio, imageRect.rect.height);
        } else {
            // The image is constrained by its width.
            var imageHeight = (imageAspectRatio / rectAspectRatio) * imageRect.rect.height;
            var excessHeight = imageRect.rect.height - imageHeight;
            imageRectInLocalScreenCoordinates.Set(0, imageRect.pivot.y * excessHeight,
                imageRect.rect.width, imageAspectRatio * imageRect.rect.width);
        }

        locationRelativeToImageInScreenCoordinates.Set(
            pivotCancelledLocation.x - imageRectInLocalScreenCoordinates.x,
            pivotCancelledLocation.y - imageRectInLocalScreenCoordinates.y);
        
        locationRelativeToImage01.Set(
            locationRelativeToImageInScreenCoordinates.x / imageRectInLocalScreenCoordinates.width,
            locationRelativeToImageInScreenCoordinates.y / imageRectInLocalScreenCoordinates.height);
        
        var imageCoord = new Vector2(locationRelativeToImage01.x * image.sprite.texture.width, 
            locationRelativeToImage01.y * image.sprite.texture.height);
        
        return imageCoord;
    }
    
    Vector2 GetRectSpaceCoordinateFromPixel(Vector2 imageCoord, Image image, RectTransform imageRect) {
        if (image.sprite == null) {
            Err("Image not loaded");
            return default;
        }
        
        var locationRelativeToImage01 = new Vector2(imageCoord.x / image.sprite.texture.width, imageCoord.y / image.sprite.texture.height);
        var imageAspectRatio = image.sprite.rect.height / image.sprite.rect.width;
        var rectAspectRatio = imageRect.rect.height / imageRect.rect.width;
        var imageRectInLocalScreenCoordinates = new Rect();
        if (imageAspectRatio > rectAspectRatio) {
            // The image is constrained by its height.
            var imageWidth = (rectAspectRatio / imageAspectRatio) * imageRect.rect.width;
            var excessWidth = imageRect.rect.width - imageWidth;
            imageRectInLocalScreenCoordinates.Set(imageRect.pivot.x * excessWidth, 0,
                imageRect.rect.height / imageAspectRatio, imageRect.rect.height);
        } else {
            // The image is constrained by its width.
            var imageHeight = (imageAspectRatio / rectAspectRatio) * imageRect.rect.height;
            var excessHeight = imageRect.rect.height - imageHeight;
            imageRectInLocalScreenCoordinates.Set(0, imageRect.pivot.y * excessHeight,
                imageRect.rect.width, imageAspectRatio * imageRect.rect.width);
        }
        
        var locationRelativeToImageInScreenCoordinates = new Vector2(imageRectInLocalScreenCoordinates.width * locationRelativeToImage01.x,
            imageRectInLocalScreenCoordinates.height * locationRelativeToImage01.y);
        var pivotCancelledLocation = new Vector2(imageRectInLocalScreenCoordinates.x + locationRelativeToImageInScreenCoordinates.x, 
            imageRectInLocalScreenCoordinates.y + locationRelativeToImageInScreenCoordinates.y);
        
        var pos = new Vector2(pivotCancelledLocation.x + imageRect.rect.x, pivotCancelledLocation.y + imageRect.rect.y);

        return pos;
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
        _config.Image1.sprite = _image1;
        _config.Image2.sprite = _image2;
    }
    
    [Button, ShowIf(nameof(IsPlaymode))]
    void LoadJson() {
        // Clear();
        
        var path = EditorUtility.OpenFilePanel("Load file", "", "json");
        if (!File.Exists(path)) {
            Err(path + " Not exist!");
            return;
        }

        var jsonString = File.ReadAllText(path);
        
        var data = JsonUtility.FromJson<Data>(jsonString);
        
        LoadAndCreateImages(data);
    }

    void LoadAndCreateImages(Data data) {
        // Load image through web request
        foreach (var point in data.Points) {
            CreateHandlerFromPoint(new Vector2(point.X, point.Y));
        }
    }

    void CreateHandlerFromPoint(Vector2 point) {
        var localPos1 =
            GetRectSpaceCoordinateFromPixel(point, _config.Image1, _config.Image1.GetComponent<RectTransform>());

        CreateHandler(localPos1, point, _config.Image1, _currentHandlerId);

        var localPos2 =
            GetRectSpaceCoordinateFromPixel(point, _config.Image2, _config.Image2.GetComponent<RectTransform>());


        CreateHandler(localPos2, point, _config.Image2, _currentHandlerId);

        _currentHandlerId++;
    }

    [Button, ShowIf(nameof(IsPlaymode))]
    void Clear() {
        _config.Image1.sprite = null;
        _config.Image2.sprite = null;
        _config.Image1.transform.DestroyAllChildren();
        _config.Image2.transform.DestroyAllChildren();
        _handlers.Clear();
        _currentHandlerId = 0;
    }
    
    [Button, ShowIf(nameof(IsPlaymode))]
    void SaveJson() {
        var data = new Data();
        var points = new List<Point>();
        var uniq = _handlers.DistinctBy(handler => handler.Id);
        
        foreach (var handler in uniq.ToArray()) {
            points.Add(new Point() {
                X = handler.ImageSpaceCoordinates.x,
                Y = handler.ImageSpaceCoordinates.y,
                Radius = handler.Radius
            });
        }

        data.Points = points.ToArray();

        // copy image to streaming assets
        // set path
        
        var jsonString = JsonUtility.ToJson(data);
        var path = EditorUtility.SaveFilePanelInProject("Save json", _image1.texture.name, "json", "Save json");
        File.WriteAllText(path, jsonString);
        AssetDatabase.Refresh();
    }

    [System.Serializable]
    struct Data {
        public string Image1Path;
        public string Image2Path;
        public Point[] Points;
    }

    [System.Serializable]
    struct Point {
        public float X;
        public float Y;
        public float Radius;
    }

    void Err(string message) {
        Debug.LogError($"[{GetType()}] {message}");
    }
    
   
}

public static class Extension {
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector) {
        var knownKeys = new HashSet<TKey>();
        return source.Where(element => knownKeys.Add(keySelector(element)));
    }
}

