using System;

[Serializable]
public struct Data {
    public string Image1Path;
    public string Image2Path;
    public Point[] Points;
}

[Serializable]
public struct Point {
    public float X;
    public float Y;
    public float Radius;

    public override string ToString() {
        return $"x: {X} y: {Y} Radius: {Radius}";
    }
}