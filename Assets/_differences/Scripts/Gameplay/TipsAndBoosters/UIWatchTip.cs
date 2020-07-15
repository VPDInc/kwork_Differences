using UnityEngine;

using Zenject;

public class UIWatchTip : Tip {
    [Inject] UITimer _timer = default;

    [SerializeField] float _timeBoost = 45f;
    
    protected override bool TryActivate() {
        _timer.AddTime(_timeBoost);
        return true;
    }
}
