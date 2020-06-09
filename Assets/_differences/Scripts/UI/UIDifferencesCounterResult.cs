using UnityEngine;
using UnityEngine.UI;

public class UIDifferencesCounterResult : MonoBehaviour {
    [SerializeField] Sprite _unlockedSprite = default;
    [SerializeField] Sprite _lockedSprite = default;
    [SerializeField] Image _icon = default;
    
    public void Setup(bool toggle) {
        _icon.sprite = toggle ? _unlockedSprite : _lockedSprite;
    }
}