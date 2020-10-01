using UnityEngine;

public static class CollisionUtils {
    public static bool TestPointRectangle(Vector2 pixel, Point point) {
        var rect = new Rect(Vector2.zero, new Vector2(point.Width, point.Height));
        rect.center = point.Center;
        return IsRectContainsPoint(rect, point.Rotation, pixel);
    }

    static bool IsRectContainsPoint(Rect rect, float rectAngle, Vector2 point) {
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

    public static bool TestPointEllipse(Vector2 point, Vector2 center, float width, float height, float angle) {
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

    public static bool TestCircleRect(Vector2 circleCenter, float circleRadius, Vector2 rectangleCenter,
        float width, float height, float rotation) {
        var bounds = CalculateBounds(rectangleCenter, width, height, rotation);

        return IsPointInRectangle(circleCenter, bounds) ||
               IsLineIntersectCircle(circleCenter, circleRadius, bounds[0], bounds[1]) ||
               IsLineIntersectCircle(circleCenter, circleRadius, bounds[1], bounds[2]) ||
               IsLineIntersectCircle(circleCenter, circleRadius, bounds[2], bounds[3]) ||
               IsLineIntersectCircle(circleCenter, circleRadius, bounds[3], bounds[0]);
    }

    static Vector2[] CalculateBounds(Vector2 rectangleCenter, float width, float height, float rotation) {
        var bounds = new Vector2[4];

        var halfWidth = width * 0.5f;
        var halfHeight = height * 0.5f;
        var angle = rotation * Mathf.Deg2Rad;

        bounds[0] = RotatePoint(rectangleCenter, -halfWidth, halfHeight, angle);
        bounds[1] = RotatePoint(rectangleCenter, halfWidth, halfHeight, angle);
        bounds[2] = RotatePoint(rectangleCenter, halfWidth, -halfHeight, angle);
        bounds[3] = RotatePoint(rectangleCenter, -halfWidth, -halfHeight, angle);

        return bounds;
    }

    static Vector2 RotatePoint(Vector2 center, float x, float y, float angle) {
        // x′=xcosθ−ysinθ
        // y′=ycosθ+xsinθ
        var point = new Vector2(x * Mathf.Cos(angle) - y * Mathf.Sin(angle),
            x * Mathf.Sin(angle) + y * Mathf.Cos(angle));

        point.x += center.x;
        point.y += center.y;

        return point;
    }

    static bool IsPointInRectangle(Vector2 circleCenter, Vector2[] bounds) {
        var ap = bounds[0] - circleCenter;
        var ab = bounds[0] - bounds[1];
        var ad = bounds[0] - bounds[3];
        var isPointInsideRect = 0 <= Vector2.Dot(ap, ab) && Vector2.Dot(ap, ab) <= Vector2.Dot(ab, ab) &&
                                0 <= Vector2.Dot(ap, ad) && Vector2.Dot(ap, ad) <= Vector2.Dot(ad, ad);

        return isPointInsideRect;
    }

    static bool IsLineIntersectCircle(Vector2 center, float radius, Vector2 p1, Vector2 p2) {
        var isCorner1InCircle = Vector2.Distance(center, p1) <= radius;
        var isCorner2InCircle = Vector2.Distance(center, p2) <= radius;

        var x1 = p1.x;
        var y1 = p1.y;
        var x2 = p2.x;
        var y2 = p2.y;
        var x3 = center.x;
        var y3 = center.y;

        var px = x2 - x1;
        var py = y2 - y1;
        var dAB = px * px + py * py;

        var u = ((x3 - x1) * px + (y3 - y1) * py) / dAB;
        var x = x1 + u * px;
        var y = y1 + u * py;

        var d = new Vector2(x, y);

        var isPointBetween = Mathf.Approximately(Vector2.Distance(p1, d) + Vector2.Distance(p2, d),
            Vector2.Distance(p1, p2));
        var isInRadius = Vector2.Distance(center, d) <= radius;

        return (isPointBetween && isInRadius) || isCorner1InCircle || isCorner2InCircle;
    }
}
    

