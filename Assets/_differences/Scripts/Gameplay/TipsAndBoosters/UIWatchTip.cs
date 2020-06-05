using UnityEngine;

using Zenject;

public class UIWatchTip : MonoBehaviour {
    [Inject] UITimer _timer = default;

    [SerializeField] float _timeBoost = 45f;
    
    public void OnButtonClick() {
        _timer.AddTime(_timeBoost);
    }
}
