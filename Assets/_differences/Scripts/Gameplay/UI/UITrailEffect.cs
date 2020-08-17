using DG.Tweening;

using UnityEngine;
using UnityEngine.UI.Extensions;

public class UITrailEffect : MonoBehaviour {
    [SerializeField] float _moveDuration = 0.5f;
    [SerializeField] float _scaleDuration = 0.2f;

    public void Setup(Vector2 target) {
        var seq = DOTween.Sequence(); 
        seq.Append(transform.DOMove(target, _moveDuration));
        seq.Append(transform.DOScale(0, _scaleDuration));
        seq.AppendCallback(() => Destroy(gameObject));
    }
}