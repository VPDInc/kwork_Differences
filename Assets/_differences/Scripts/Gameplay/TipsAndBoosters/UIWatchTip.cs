using UnityEngine;

using Zenject;

public class UIWatchTip : Tip {
    [Inject] UITimer _timer = default;

    [SerializeField] float _timeBoost = 45f;
    
    public override void OnButtonClick() {
        base.OnButtonClick();
        _timer.AddTime(_timeBoost);
    }
}
