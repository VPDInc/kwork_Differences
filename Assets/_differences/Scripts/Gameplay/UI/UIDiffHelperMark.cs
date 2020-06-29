using UnityEngine;
using UnityEngine.UI;

public class UIDiffHelperMark : MonoBehaviour {
    [SerializeField] Image _visual = default;

    void Awake() {
        _visual.gameObject.SetActive(false);
    }

    public void Open() {
        _visual.gameObject.SetActive(true);
    }
}
