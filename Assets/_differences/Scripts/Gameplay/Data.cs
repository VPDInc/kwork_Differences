using _differences.Scripts.PVPBot;
using System;

using UnityEngine;

[Serializable]
public struct Data {
    public string Image1Path;
    public string Image2Path;
    public Point[] Points;
    public Orientation Orientation;
    public string Id;
    public Storage Storage;
    public int CountPlayers;
    public int CountRounds => CountPlayers - 1;
    public BotDifficulty MinBotDifficulty;
    public BotDifficulty MaxBotDifficulty;
    public int FindDifferenceSeconds;
    public int TotalSecondsOnRound => (PointCount * FindDifferenceSeconds) * CountRounds;

    public int PointCount => Points?.Length ?? 0;
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