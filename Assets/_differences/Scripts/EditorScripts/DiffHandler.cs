using System;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

public class DiffHandler : MonoBehaviour {
    [ReadOnly] public Vector2 ImageSpaceCoordinates;
    [ReadOnly] public int Id;
    
    [ShowInInspector, ReadOnly]
    public float Radius { get; private set; } = DEFAULT_SIZE;
    public bool IsSelected {
        set {
            _isSelected = value;
            _visual.color = value ? _selected : _notSelected;
        }
        
        get => _isSelected;
    }
    
    [SerializeField] Image _visual = default;
    [SerializeField] Color _notSelected = Color.black;
    [SerializeField] Color _selected = Color.red;

    bool _isSelected = false;

    const float DEFAULT_SIZE = 100;

    void Awake() {
        SetRadius(DEFAULT_SIZE, DEFAULT_SIZE);
    }

    public void SetRadius(float spriteSpace, float imageSpace) {
        Radius = spriteSpace;
        _visual.GetComponent<RectTransform>().sizeDelta = new Vector2(imageSpace, imageSpace);
    }
}
