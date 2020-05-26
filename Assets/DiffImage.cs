using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEditor.Experimental.GraphView;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DiffImage : MonoBehaviour {
    [SerializeField] Sprite _image1 = default;
    [SerializeField] Sprite _image2 = default;

    Image _diff1;
    Image _diff2;

    bool IsPlaymode => Application.isPlaying;
    
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
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(imageRect, mousePos, null,
                    out var localPoint)) {
                    var imageCoords = GetPixelSpaceCoordinate(localPoint, image, imageRect);
                }
            }
        }
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

    Vector2 GetPixelSpaceCoordinate(Vector2 pos, Image image, RectTransform imageRect) {
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

    Vector2 GetRectSpaceCoordinate(RectTransform imageRect, Vector2 localPoint) {
        var normalizedPoint = Rect.PointToNormalized(imageRect.rect, localPoint);
        var rectCoord = new Vector2(imageRect.rect.width * normalizedPoint.x, imageRect.rect.height * normalizedPoint.y);
        return rectCoord;
    }

    void FindResources() {
        _diff1 = GameObject.Find("DiffImage1").GetComponent<Image>();
        _diff2 = GameObject.Find("DiffImage2").GetComponent<Image>();
    }

    [Button, ShowIf(nameof(IsPlaymode))]
    void Load() {
        _diff1.sprite = _image1;
        _diff2.sprite = _image2;
    }

    [Button, ShowIf(nameof(IsPlaymode))]
    void Clear() {
        _diff1.sprite = null;
        _diff2.sprite = null;
    }

    void Err(string message) {
        Debug.LogError($"[{GetType()}] {message}");
    }
}
