using TMPro;

using UnityEngine;

public class UILevel : MonoBehaviour {
    [SerializeField] string _prefix = "Level: ";

    TextMeshProUGUI _text = default;
    
    void Awake() {
        _text = GetComponent<TextMeshProUGUI>();
    }

    void Start() {
        LevelSystem.Instance.LevelLoaded += OnLevelLoaded;
        SetLevel();
    }

    void OnDestroy() {
        LevelSystem.Instance.LevelLoaded -= OnLevelLoaded;
    }

    void SetLevel() {
        _text.text = _prefix + LevelSystem.Instance.LevelNumPretty;
    }
    
    void OnLevelLoaded(Level level) {
        SetLevel();
    } 
}
