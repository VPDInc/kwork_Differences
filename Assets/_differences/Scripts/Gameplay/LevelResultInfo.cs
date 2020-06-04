using System.Collections.Generic;

using Airion.Extensions;

using JetBrains.Annotations;

using UnityEngine;

public class LevelResultInfo {
    List<PictureResult> _pictureResults = new List<PictureResult>();
    bool _isCompleted = false;

    public bool IsCompleted => _isCompleted;

    public List<PictureResult> PictureResults => _pictureResults;

    public LevelResultInfo(bool isCompleted,
                           List<PictureResult> pictureResults = null) {
        _isCompleted = isCompleted;
        _pictureResults = pictureResults;
    }
    
    //DUMMY
    public LevelResultInfo() {
        FillRandomDifferencesInfo();
    }

    void FillRandomDifferencesInfo() {
        var picturesCount = Random.Range(1,3);
        var differencesCount = Random.Range(5, 8);
        _isCompleted = RandomExtensions.TryChance(0.75f);

        for (int i = 0; i < picturesCount; i++) {
            var pictureResult = new PictureResult(null, new List<DifferencePoint>());
            for (int j = 0; j < differencesCount; j++) {
                bool differenceGuess = i < picturesCount - 1 || _isCompleted || RandomExtensions.TryChance(0.75f);
                pictureResult.AddPoint(differenceGuess);
            }
            _pictureResults.Add(pictureResult);
        }
    }
}

public class PictureResult {
    Sprite _picture = default;
    List<DifferencePoint> _differencePoints = new List<DifferencePoint>();

    public Sprite Picture => _picture;
    public List<DifferencePoint> DifferencePoints => _differencePoints;

    public PictureResult(Sprite picture, List<DifferencePoint> differencePoints) {
        _picture = picture;
        _differencePoints = differencePoints;
    }

    public void AddPoint(bool isOpen) {
        _differencePoints.Add(new DifferencePoint(isOpen));
    }
}

public class DifferencePoint {
    bool _isOpen = false;

    public bool IsOpen => _isOpen;

    public DifferencePoint(bool isOpen) {
        _isOpen = isOpen;
    }
}