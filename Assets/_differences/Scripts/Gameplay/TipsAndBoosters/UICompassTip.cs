using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UICompassTip : Tip {
    [SerializeField] CompassVisual _compassVisualPrefab = default;

    [Inject] UIGameplay _gameplay = default;
    
    public override void OnButtonClick() {
        base.OnButtonClick();
        var points = _gameplay.ClosedPoints;
        if (points != null && points.Length > 0) {
            var images = _gameplay.CurrentImages;
            var size = new Vector2(images.Item1.sprite.texture.width, images.Item1.sprite.texture.height);
            var quarter = GetBetterQuarter(points, size);
            var center = GetQuarterCenter(quarter, size);
            ShowTip(center, images, size);
        }
    }

    void ShowTip(Vector2 center, (Image, Image) images, Vector2 size) {
        CreateTip(center, images.Item1, size);
        CreateTip(center, images.Item2, size);
    }
    
    void CreateTip(Vector2 center, Image image, Vector2 size) {
        var halfSize = size * 0.5f;
        var handler = Instantiate(_compassVisualPrefab);
        handler.Show();
        var handlerRect = handler.GetComponent<RectTransform>();
        var imageRect = image.GetComponent<RectTransform>();
        var pos = DiffUtils.GetRectSpaceCoordinateFromPixel(center, image, imageRect);
        handlerRect.SetParent(image.transform, false);
        handlerRect.sizeDelta = new Vector2(DiffUtils.PixelWidthToRect(halfSize.x, imageRect, image.sprite), 
            DiffUtils.PixelHeightToRect(halfSize.y, imageRect, image.sprite));
        handlerRect.localPosition = pos;
    }

    Vector2 GetQuarterCenter(int quarter, Vector2 size) {
        var halfSize = size * 0.5f;
        switch (quarter) {
            case 0: return new Vector2(halfSize.x * 0.5f, halfSize.y * 0.5f);
            case 1: return new Vector2(halfSize.x + halfSize.x * 0.5f, halfSize.y * 0.5f);
            case 2: return new Vector2(halfSize.x * 0.5f, halfSize.y + halfSize.y * 0.5f);
            case 3: return new Vector2(halfSize.x + halfSize.x * 0.5f, halfSize.y + halfSize.y * 0.5f);
        }

        return default;
    }

    int GetBetterQuarter(Point[] points, Vector2 size) {
        var quarters = new Dictionary<int,int> {{0,0},{1,0},{2,0},{3,0}};
        foreach (var point in points) {
            quarters[GetQuarter(point, size)] += 1;
        }

        var best = quarters.OrderByDescending(v => v.Value).First().Key;
        return best;
    }

    int GetQuarter(Point point, Vector2 size) {
        var halfSize = size * 0.5f;
        if (point.Center.x < halfSize.x && point.Center.y <= halfSize.y)
            return 0;
        else if (point.Center.x >= halfSize.x && point.Center.y <= halfSize.y)
            return 1;
        else if (point.Center.x < halfSize.x && point.Center.y > halfSize.y)
            return 2;
        else
            return 3;
    }
}
