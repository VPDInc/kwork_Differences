using System.Collections.Generic;
using System.Linq;

using Airion.Extensions;

using JetBrains.Annotations;

using UnityEngine;

public class GameplayResult {
    public readonly PictureResult[] PictureResults;
    public readonly int DifferencesCount;
    public readonly bool IsCompleted;
    public readonly int PicturesCount;

    public GameplayResult(bool isCompleted, [NotNull] PictureResult[] pictureResults) {
        PicturesCount = pictureResults.Length;
        DifferencesCount = pictureResults.Sum(res => res.DifferencePoints.Length);
        IsCompleted = isCompleted;
        PictureResults = pictureResults;
    }
    
    public GameplayResult() {
        PicturesCount = Random.Range(1,3);
        DifferencesCount = Random.Range(5, 8);
        PictureResults = FillRandomDifferencesInfo();
        IsCompleted = RandomExtensions.TryChance();
    }

    PictureResult[] FillRandomDifferencesInfo() {
        var results = new List<PictureResult>();
        for (int i = 0; i < PicturesCount; i++) {
            var pictureResult = new PictureResult();
            for (int j = 0; j < DifferencesCount; j++) {
                bool differenceGuess = i < PicturesCount - 1 || RandomExtensions.TryChance(0.75f);
                pictureResult.AddPoint(differenceGuess);
            }
            results.Add(pictureResult);
        }

        return results.ToArray();
    }
}

public struct PictureResult {
    public Sprite Picture;
    public DifferencePoint[] DifferencePoints;

    public void AddPoint(bool isOpen) {
        var points = DifferencePoints.ToList();
        points.Add(new DifferencePoint() {
            IsOpen = isOpen
        });
        DifferencePoints = points.ToArray();
    }
}

public struct DifferencePoint {
    public bool IsOpen;
}