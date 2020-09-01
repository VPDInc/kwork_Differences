using DG.Tweening;

using TMPro;

using UnityEngine;

using Zenject;

public class UICompleteView : MonoBehaviour {
    [SerializeField] CanvasGroup _group = default;
    [SerializeField] TextMeshProUGUI _text = default;
    [SerializeField] GameObject _checkMark = default;
    [SerializeField] Transform _medals = default;
    
    [Inject] UIMedalEarningFX _medalEarningFx = default;

    void Awake() {
        _group.alpha = 0;
    }

    public void Show(int medalsEarn) {
        _checkMark.SetActive(false);
        _checkMark.transform.localScale = Vector3.one;
        _text.text = string.Empty;
        var seq = DOTween.Sequence();
        
        seq.AppendInterval(1);
        
        seq.Append(_group.DOFade(1, 0.5f));

        seq.AppendCallback(() => {
            _checkMark.SetActive(true);
        });
        
        seq.Append(DOTween.To(() => _text.text, x => _text.text = x, "Completed",
            0.5f));
        
        seq.AppendCallback(() => {
            _medalEarningFx.CallEffect(_medals.position, medalsEarn);
        });
        
        seq.AppendInterval(2f);
        seq.Append(_group.DOFade(0, 0.5f));
    }
}
