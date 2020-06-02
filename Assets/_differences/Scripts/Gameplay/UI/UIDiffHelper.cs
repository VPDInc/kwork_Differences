using System.Collections.Generic;

using Airion.Extensions;

using TMPro;

using UnityEngine;

public class UIDiffHelper : MonoBehaviour {
    [SerializeField] float _delta = 1;
    [SerializeField] float _startSize = 100;
    [SerializeField] RectTransform _back = default;
    [SerializeField] RectTransform _content = default;
    
    [SerializeField] TextMeshProUGUI _amountText = default;
    [SerializeField] UIDiffHelperMark _markPrefab = default;

    readonly List<UIDiffHelperMark> _marks = new List<UIDiffHelperMark>();
    
    public void SetPointsAmount(int amount) {
        _content.DestroyAllChildren();
        _marks.Clear();
        
        _amountText.text = amount.ToString();
        _back.sizeDelta = new Vector2(_startSize + _delta * (amount + 1), _back.sizeDelta.y);
        for (int i = 0; i < amount; i++) {
            var mark = Instantiate(_markPrefab, _content);
            var x = _delta * i - ((_delta * amount * 0.5f) - _delta); 
            mark.transform.localPosition = new Vector2(x, 0);
            _marks.Add(mark);
        }
    }
    
    [ContextMenu("1")]
    void Set2() {
        SetPointsAmount(5);
    }

    public void Open(int index) {
        
    }
}
