using System.Collections.Generic;

using Airion.Extensions;

using UnityEngine;

public class LevelResultInfo {
    int _picturesCount = 0;
    int _differencesCount = 0;
    List<Dictionary<int, bool>> _differencesGuesses = new List<Dictionary<int, bool>>();

    public int PicturesCount => _picturesCount;

    public int DifferencesCount => _differencesCount;

    public List<Dictionary<int, bool>> DifferencesGuesses => _differencesGuesses;

    public LevelResultInfo(int picturesCount,
                           int differencesCount,
                           List<Dictionary<int, bool>> differencesGuesses = null) {
        _picturesCount = picturesCount;
        _differencesCount = differencesCount;
        
        if(differencesGuesses != null)
            _differencesGuesses = differencesGuesses;
    }
    
    //DUMMY
    public LevelResultInfo() {
        _picturesCount = Random.Range(1,3);
        _differencesCount = Random.Range(5, 8);
        
        FillRandomDifferencesInfo();
    }

    public void AddPictureDifferences(Dictionary<int, bool> differences) {
        _differencesGuesses.Add(differences);
    }

    void FillRandomDifferencesInfo() {
        for (int i = 0; i < _picturesCount; i++) {
            Dictionary<int, bool> differencesInfo = new Dictionary<int, bool>();
            for (int j = 0; j < _differencesCount; j++) {
                bool differenceGuess = i < _picturesCount - 1 || RandomExtensions.TryChance(0.75f);
                differencesInfo.Add(j, differenceGuess);
            }
            AddPictureDifferences(differencesInfo);
        }
    }
}