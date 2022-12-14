using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class DiffUtils {
    public static Data Parse(string dataString) {
        // TODO: Check possible incompatible data
        return JsonUtility.FromJson<Data>(dataString);
    } 
    
    public static Vector2 GetRectSpaceCoordinateFromPixel(Vector2 imageCoord, Image image, RectTransform imageRect) {
        if (image.sprite == null) {
            Debug.LogError($"[{nameof(DiffUtils)}] Image not loaded!");
            return default;
        }
        
        var locationRelativeToImage01 = new Vector2(imageCoord.x / image.sprite.rect.width, imageCoord.y / image.sprite.rect.height);
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

    public static bool GetPixelFromScreen(Vector2 screenPos, Image image, out Vector2 pixelsSpace, out Vector2 localSpace) {
        var imageRect = image.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(imageRect, screenPos, null, out var pos)) {
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
        
            var imageCoord = new Vector2(locationRelativeToImage01.x * image.sprite.rect.width, 
                locationRelativeToImage01.y * image.sprite.rect.height);

            localSpace = pos;
            pixelsSpace = imageCoord;
            return true;
        }
        
        pixelsSpace = Vector2.zero;
        localSpace = Vector2.zero;
        return false;
    }

    public static RaycastResult RaycastMouse(Vector2 position) {
        var pointerData = new PointerEventData(EventSystem.current) {
            pointerId = -1,
        };

        pointerData.position = position;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results.FirstOrDefault();
    }

    public static float PixelHeightToRect(float pixel, RectTransform rect, Sprite sprite) {
        return pixel * (rect.rect.height / sprite.rect.height);
    }
    
    public static float PixelWidthToRect(float pixel, RectTransform rect, Sprite sprite) {
        return pixel * (rect.rect.width / sprite.rect.width);
    }
    
    public static Point FixPointRelative(Point point, Sprite sprite) {
        return new Point() {
            Center = new Vector2(point.Center.x * sprite.rect.width, point.Center.y * sprite.rect.height),
            Height = point.Height * sprite.rect.height,
            Width = point.Width * sprite.rect.width,
            Number = point.Number,
            Rotation = point.Rotation,
            Shape = point.Shape
        };
    }
}
