using System;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class DiffHandler : MonoBehaviour {
    [ReadOnly] public Vector2 ImageSpaceCoordinates;
    [ReadOnly] public int Id;
    
    [ShowInInspector, ReadOnly]
    public float Height { get; private set; } = DEFAULT_SIZE;
    [ShowInInspector, ReadOnly]
    public float Width { get; private set; } = DEFAULT_SIZE;
    public Shape Shape { get; private set; } = Shape.Rectangle;

    public bool IsSelected {
        set {
            _isSelected = value;
            _visual.color = value ? _selected : _notSelected;
        }
        
        get => _isSelected;
    }
    
    [ShowInInspector, ReadOnly]
    public int Number {
        get => _number;
        set {
            _number = value;
            _numberText.text = _number.ToString();
        }
    }
    
    [SerializeField] Image _visual = default;
    [SerializeField] TextMeshProUGUI _numberText = default;
    [SerializeField] Color _notSelected = Color.black;
    [SerializeField] Color _selected = Color.red;
    [SerializeField] Sprite _rectangle = default;
    [SerializeField] Sprite _circle = default;

    bool _isSelected = false;
    int _number = 0;
    
    const float DEFAULT_SIZE = 50;

    void Awake() {
        SetWidth(DEFAULT_SIZE);
        SetHeight(DEFAULT_SIZE);
    }

    public void SetWidth(float spriteSpace) {
        Width = spriteSpace;
        var parentRect = transform.parent.GetComponent<RectTransform>();
        var sprite = transform.parent.GetComponent<Image>().sprite;
        var imageSpace = DiffUtils.PixelWidthToRect(spriteSpace, parentRect, sprite);
        var visualRect = _visual.GetComponent<RectTransform>();
        var sizeDelta = visualRect.sizeDelta;
        sizeDelta.Set(imageSpace, visualRect.sizeDelta.y);
        visualRect.sizeDelta = sizeDelta;
    }
    
    public void SetHeight(float spriteSpace) {
        Height = spriteSpace;
        var parentRect = transform.parent.GetComponent<RectTransform>();
        var sprite = transform.parent.GetComponent<Image>().sprite;
        var imageSpace = DiffUtils.PixelHeightToRect(spriteSpace, parentRect, sprite);
        var visualRect = _visual.GetComponent<RectTransform>();
        var sizeDelta = visualRect.sizeDelta;
        sizeDelta.Set(visualRect.sizeDelta.x, imageSpace);
        visualRect.sizeDelta = sizeDelta;
    }

    public void SetShape(Shape shape) {
        Shape = shape;
        switch (shape) {
            case Shape.Rectangle:
                _visual.sprite = _rectangle;
                break;
            case Shape.Circle:
                _visual.sprite = _circle;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(shape), shape, null);
        }
    }
}
