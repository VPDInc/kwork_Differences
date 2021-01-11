using DG.Tweening;
using System;
using UnityEngine;

public class UITrailEffect : MonoBehaviour 
{
    [SerializeField] float _moveDuration = 0.5f;
    [SerializeField] float _scaleDuration = 0.2f;

    public void Setup(Vector2 target, float delay = 0, Action OnSuccess = null) 
    {
        var seq = DOTween.Sequence();
        seq.AppendInterval(delay);
        seq.Append(transform.DOMove(target, _moveDuration));
        seq.Append(transform.DOScale(0, _scaleDuration));
        seq.AppendCallback( delegate { Destroy(gameObject); OnSuccess?.Invoke(); });
    }
}