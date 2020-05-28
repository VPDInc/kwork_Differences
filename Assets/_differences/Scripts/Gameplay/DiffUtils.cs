using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
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
    
    public static Vector2 GetPixelSpaceCoordinateFromRectPoint(Vector2 pos, Image image, RectTransform imageRect) {
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
    
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector) {
        var knownKeys = new HashSet<TKey>();
        return source.Where(element => knownKeys.Add(keySelector(element)));
    }
}
