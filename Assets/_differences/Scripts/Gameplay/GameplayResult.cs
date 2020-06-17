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
    public Sprite Picture;
    public DifferencePoint[] DifferencePoints;
    public int StarsCollected;
}

public struct DifferencePoint {
    public bool IsOpen;
}