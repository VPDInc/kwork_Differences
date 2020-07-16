using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class UIPictureLayout : MonoBehaviour {
    public int HorizontalCount => _horizontalPictures.Length;
    public int VerticalCount => _verticalPictures.Length;
    
    [SerializeField] Image[] _horizontalPictures = default;
    [SerializeField] Image[] _verticalPictures = default;
}