using System.Collections.Generic;

using Airion.Extensions;

using JetBrains.Annotations;

using UnityEngine;

public class LevelResultInfo {
    List<PictureResult> _pictureResults = new List<PictureResult>();
    int _picturesCount = 0;
    int _differencesCount = 0;
    bool _isCompleted = false;

    public int PicturesCount => _picturesCount;
    
    public int DifferencesCount => _differencesCount;

    public bool IsCompleted => _isCompleted;

    public List<PictureResult> PictureResults => _pictureResults;

    public LevelResultInfo(int picturesCount,
                           int differencesCount,
                           bool isCompleted,
                           List<PictureResult> pictureResults = null) {
        _picturesCount = picturesCount;
        _differencesCount = differencesCount;
        _isCompleted = isCompleted;
        _pictureResults = pictureResults;
    }
    
    //DUMMY
    public LevelResultInfo() {
        _picturesCount = Random.Range(1,3);
        _differencesCount = Random.Range(5, 8);

        FillRandomDifferencesInfo();
    }

    void FillRandomDifferencesInfo() {
        for (int i = 0; i < _picturesCount; i++) {
            var pictureResult = new PictureResult(null, new List<DifferencePoint>());
            for (int j = 0; j < _differencesCount; j++) {
                bool differenceGuess = i < _picturesCount - 1 || RandomExtensions.TryChance(0.75f);
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