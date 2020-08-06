using DG.Tweening;

using UnityEngine;

public class SimpleCycleMover : MonoBehaviour {
    [SerializeField] Vector3 _target = default;
    [SerializeField] float _speed = 1;
    [SerializeField] Ease _ease = default;
    [SerializeField] LoopType _loopType = default;
    [SerializeField] bool _isLocal = false;

    void Start() {
        if (_isLocal) {
            transform.DOLocalMove(_target, 1 / _speed).SetEase(_ease).SetLoops(-1, _loopType);
        } else {
            transform.DOMove(transform.position + _target, 1 / _speed).SetEase(_ease).SetLoops(-1, _loopType);
        }
    }

    void OnDisable() {
        transform.DOKill();
    }
}