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

    PicturePanel _panel;

    void Awake() {
        _panel = GetComponentInParent<PicturePanel>();
    }

    public void SetPicture(Orientation orientation, Sprite sprite, bool isCompleted) {
        if (orientation == Orientation.Horizontal) {
            _horizontalPictures[_currentHorizontalPictures].sprite = isCompleted ? sprite : _panel.HorizontalDummy;
            _currentHorizontalPictures++;
        } else {
            _verticalPictures[_currentVerticalPictures].sprite = isCompleted ? sprite : _panel.VerticalDummy;
            _currentVerticalPictures++;
        }
    }

    public void Reset() {
        _currentHorizontalPictures = 0;
        _currentVerticalPictures = 0;
    }
}