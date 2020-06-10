using System;

using UnityEngine;

[Serializable]
public struct Data {
    public string Image1Path;
    public string Image2Path;
    public Point[] Points;
    public Orientation Orientation;
    public string Id;
}

[Serializable]
public struct Point {
    public Vector2 Center;
    public float Width;
    public float Height;
    public int Number;
    public Shape Shape;
    public float Rotation;

    public override string ToString() {
        return $"Center: {Center} Width: {Width} Height: {Height} Shape: {Shape} Rotation: {Rotation}";
    }
}

[Serializable]
public enum Orientation {
    Vertical,
    Horizontal
}