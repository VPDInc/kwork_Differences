using System;
using System.Collections;

using DG.Tweening;

using UnityEngine;

using Random = UnityEngine.Random;

public class CloudAnimation : MonoBehaviour {
    const float MAX_MOVING_RADIUS = 0.8f;
    const float MIN_MOVING_RADIUS = 0.4f;

    const float MIN_MOVING_DURATION = 7;
    const float MAX_MOVING_DURATION = 13;

    const float BLOCK_DISSOLVE_EFFECT_DURATION = 0.5f;

    Vector3 _startPosition;
    SpriteRenderer _spriteRenderer = default;

    void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    IEnumerator Start() {
        _startPosition = transform.position;
         while (gameObject.activeInHierarchy) {
             var nextPoint = _startPosition + Random.onUnitSphere * Random.Range(MIN_MOVING_RADIUS, MAX_MOVING_RADIUS);
             var duration = Random.Range(MIN_MOVING_DURATION, MAX_MOVING_DURATION);
             yield return transform.DOMove(nextPoint, duration).SetEase(Ease.Linear).WaitForCompletion();
         }
    }

     void OnDestroy() {
         transform.DOKill();
         _spriteRenderer.DOKill();
     }

     public void Hide(bool isInstant) {
         _spriteRenderer.DOFade(0, isInstant ? 0 : BLOCK_DISSOLVE_EFFECT_DURATION).OnComplete(() => {
             transform.DOKill();
             gameObject.SetActive(false);
         });
     }
}
