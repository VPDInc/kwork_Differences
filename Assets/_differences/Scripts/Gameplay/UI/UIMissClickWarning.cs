using TMPro;

using UnityEngine;

public class UIMissClickWarning : MonoBehaviour {
    [SerializeField] TextMeshProUGUI _text = default;
    
    public void SetReducedTimeAndRun(float reduceTime) {
        _text.text = $"-{reduceTime} seconds!";
        Destroy(gameObject, 1.5f);           
    }
}
