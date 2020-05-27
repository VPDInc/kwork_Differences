using DG.Tweening;

using UnityEngine;

public class StarHandler : MonoBehaviour {
    [SerializeField] Transform[] _stars = default;
    [SerializeField] SpriteRenderer[] _starRenderers = default;

    const float VFX_DURATION = 0.2f;

    void Awake() {
        Reset();
    }

    public void Show(bool toggle, bool isInstant) {
        foreach (SpriteRenderer spriteRenderer in _starRenderers) {
            spriteRenderer.DOFade(toggle ? 1 : 0, isInstant ? 0 : VFX_DURATION);
        }
    }

    public void SetStars(int stars, bool isInstant) {
        DOTween.Kill(this);
        var seq = DOTween.Sequence().SetId(this);
        for (var i = 0; i < Mathf.Clamp(stars, 0, _stars.Length); i++) {
            Transform star = _stars[i];
            seq.Append(star.DOScale(1, isInstant ? 0 : VFX_DURATION));
        }
    }

    void Reset() {
        foreach (Transform star in _stars) {
            star.DOScale(0, 0);
        }
    }
}