using DG.Tweening;

using TMPro;

using UnityEngine;

public class UIMedalCounter : MonoBehaviour {
    [SerializeField] TMP_Text _label = default;
    [SerializeField] CanvasGroup _canvasGroup = default;
    [SerializeField] float _fxShowDuration = 0.25f;
    [SerializeField] float _fxPauseDuration = 0.25f;
    [SerializeField] float _fxHideDuration = 0.5f;

    const string LABEL_SUFFIX = "<sprite=0>";

    void Awake() {
        _canvasGroup.alpha = 0;
    }

    public void Setup(int medalAmount) {
        _label.text = "+" + medalAmount + LABEL_SUFFIX;
        var seq = DOTween.Sequence();
        seq.Append(_canvasGroup.DOFade(1, _fxShowDuration));
        seq.AppendInterval(_fxPauseDuration);
        seq.Append(_canvasGroup.DOFade(0, _fxHideDuration));
        seq.AppendCallback(() => Destroy(gameObject));
    }
}