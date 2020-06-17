using TMPro;

using UnityEngine;

public class UIStars : MonoBehaviour {
    [SerializeField] TextMeshProUGUI _text = default;

    int _currentAmount = 0;
    
    public void Add(int amount) {
        _currentAmount += amount;
        UpdateVisual();
    }

    public void Reset() {
        _currentAmount = 0;
        UpdateVisual();
    }

    void UpdateVisual() {
        _text.text = _currentAmount.ToString();
    }
}
