using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

public class UIStarHandler : MonoBehaviour {
    [SerializeField] Transform[] _starFilledTransforms = default;
    [SerializeField] Image[] _starImages = default;

    const float VFX_DURATION = 0.2f;

    void Awake() {
        ResetStars();
    }

    public void Show(bool toggle, bool isInstant) {
        foreach (Image starImage in _starImages) {
            starImage.DOFade(toggle ? 1 : 0, isInstant ? 0 : VFX_DURATION);
        }
    }

    public void SetStars(int stars, bool isInstant) {
        var seq = DOTween.Sequence().SetId(this);
        for (var i = 0; i < Mathf.Clamp(stars, 0, _starFilledTransforms.Length); i++) {
            Transform star = _starFilledTransforms[i];
            seq.Append(star.DOScale(1, isInstant ? 0 : VFX_DURATION));
        }
    }

    public void ResetStars() {
        foreach (Transform star in _starFilledTransforms) {
            star.DOScale(0, 0);
        }
    }
}