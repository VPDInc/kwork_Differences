using System;

using DG.Tweening;

using UnityEngine;
using UnityEngine.UI.Extensions;

public class UIShineEffectHandler : MonoBehaviour {
    [SerializeField] ShineEffector _shineEffect = default;
    [SerializeField] float _delayBetweenShines = 15;
    [SerializeField] float _fxDuration = 0.5f;

    void Awake() {
        _shineEffect = GetComponent<ShineEffector>();
    }

    void Start() {
        var seq = DOTween.Sequence().SetLoops(-1, LoopType.Restart);
        seq.AppendCallback(() => { _shineEffect.YOffset = 1; });
        seq.AppendInterval(_delayBetweenShines);
        seq.Append(DOTween.To(() => _shineEffect.YOffset, x => _shineEffect.YOffset = x, -1, _fxDuration));
    }
}