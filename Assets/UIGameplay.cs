using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIGameplay : MonoBehaviour {
    [SerializeField] Image _image1 = default;
    [SerializeField] Image _image2 = default;
    [SerializeField] TextMeshProUGUI _pointsCountText = default;
    
    [Inject] Database _database = default;
    [Inject] ImagesLoader _loader = default;

    Data _data;
    int _pointsCount;
    int _currentPointsFound = 0;

    void Start() {
        var levelData = _database.GetLevelByNum(0);
        _data = levelData;
        _loader.LoadImagesAndCreateSprite(levelData.Image1Path, levelData.Image2Path, OnLoaded);
    }
    
    
    void OnLoaded(Sprite im1, Sprite im2) {
        Fill(im1, im2, _data);
    }
    
    void Fill(Sprite image1, Sprite image2, Data levelData) {
        _image1.sprite = image1;
        _image2.sprite = image2;
        _pointsCount = levelData.Points.Length;
        _currentPointsFound = 0;
        UpdatePointsAmount();
    }

    void UpdatePointsAmount() {
        _pointsCountText.text = $"{_currentPointsFound}/{_pointsCount}";
    }

    void Update() {
        // if down
        // get pixels
        // pixel inside point with radius?
        // remove point
        // currentpoints++
        // differences.text = currentpoints/count
    }
}
