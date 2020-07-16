using System;

using UnityEngine;
using UnityEngine.UI;

public class UIPictureLayout : MonoBehaviour {
    public int HorizontalCount => _horizontalPictures.Length;
    public int VerticalCount => _verticalPictures.Length;
    
    [SerializeField] Image[] _horizontalPictures = default;
    [SerializeField] Image[] _verticalPictures = default;
    
    int _currentHorizontalPictures = 0;
    int _currentVerticalPictures = 0;

    public void SetPicture(Orientation orientation, Sprite sprite) {
        if (orientation == Orientation.Horizontal) {
            _horizontalPictures[_currentHorizontalPictures].sprite = sprite;
            _currentHorizontalPictures++;
        } else {
            _verticalPictures[_currentVerticalPictures].sprite = sprite;
            _currentVerticalPictures++;
        }
    }

    public void Reset() {
        _currentHorizontalPictures = 0;
        _currentVerticalPictures = 0;
    }
}