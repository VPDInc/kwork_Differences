using System.Linq;

using JetBrains.Annotations;

using UnityEngine;

public class GameplayResult {
    public readonly PictureResult[] PictureResults;
    public readonly bool IsCompleted;
    public readonly int PicturesCount;
    public int TotalStarsCollected => PictureResults.Sum(res => res.StarsCollected);

    public GameplayResult(bool isCompleted, [NotNull] PictureResult[] pictureResults) {
        PicturesCount = pictureResults.Length;
        IsCompleted = isCompleted;
        PictureResults = pictureResults;
    }
}

public struct PictureResult {
    public Sprite Picture1;
    public Sprite Picture2;
    public DifferencePoint[] DifferencePoints;
    public int StarsCollected;
    public Orientation Orientation;
    public (Sprite, Sprite) Pictures => (Picture1, Picture2);
}

public struct DifferencePoint {
    public bool IsOpen;
}