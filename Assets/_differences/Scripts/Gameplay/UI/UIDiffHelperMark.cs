using UnityEngine;
using UnityEngine.UI;

public class UIDiffHelperMark : MonoBehaviour {
    [SerializeField] Color _closeColor = Color.black;
    [SerializeField] Color _openColor = Color.blue;
    [SerializeField] Image _visual = default;

    void Awake() {
        _visual.color = _closeColor;
    }

    public void Open() {
        _visual.color = _openColor;
    }
}
