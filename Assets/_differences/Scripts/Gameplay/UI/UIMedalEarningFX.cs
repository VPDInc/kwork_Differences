using System.Collections;
using System.Collections.Generic;

using DG.Tweening;

using UnityEngine;

public class UIMedalEarningFX : MonoBehaviour {
    [SerializeField] RectTransform _medalPrefab = default;
    [SerializeField] RectTransform _target = default;
    [SerializeField] float _delayBetweenMedals = default;
    [SerializeField] float _flyDuration = default;
    [SerializeField] float _scaleDuration = default;

    public void CallEffect(Vector2 position, int count) {
        var seq = DOTween.Sequence();

        for (int i = 0; i < count; i++) {
            var medal = Instantiate(_medalPrefab, transform);
            medal.position = position;

            seq.Insert(_delayBetweenMedals * (i + 1), medal.DOMove(_target.position, _flyDuration).SetEase(Ease.Linear)
                            .OnComplete(() => medal.DOScale(0, _scaleDuration)
                                                   .OnComplete(() => Destroy(medal.gameObject))));
            
        }
    }
}