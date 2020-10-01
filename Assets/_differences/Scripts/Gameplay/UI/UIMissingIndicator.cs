using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

public class UIMissingIndicator : MonoBehaviour {
    [SerializeField] float _lifetime = default;
    [SerializeField] float _effectDuration = default;

    Image _image;

    void Start() {
        _image = GetComponent<Image>();
        Effect();
    }

    void Effect() {
        var seq = DOTween.Sequence();
        seq.Append(_image.DOFade(1, _effectDuration));
        seq.AppendInterval(_lifetime);
        seq.Append(_image.DOFade(0, _effectDuration));
        seq.AppendCallback(() => Destroy(gameObject));
    }
}