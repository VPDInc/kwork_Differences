using DG.Tweening;

using UnityEngine;

using Zenject;

public class UIWatchTip : Tip {
    [Inject] UITimer _timer = default;

    [SerializeField] float _timeBoost = 45f;
    [SerializeField] UITrailEffect _trail = default;
    [SerializeField] Transform _watchesTarget = default;
    
    protected override bool TryActivate() {
        var seq = DOTween.Sequence();
        seq.AppendCallback(() => {
            var trail = Instantiate(_trail, transform);
            trail.Setup(_watchesTarget.position);
        });
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() => {
            _timer.AddTime(_timeBoost);
        });
        return true;
    }
}
