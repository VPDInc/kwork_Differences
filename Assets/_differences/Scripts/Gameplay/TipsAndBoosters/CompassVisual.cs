using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

public class CompassVisual : MonoBehaviour {
    public void Show() {
        var canvasGroup = GetComponent<CanvasGroup>();
        var seq = DOTween.Sequence().SetId(this);
        seq.Append(canvasGroup.DOFade(0, 1f).SetEase(Ease.Linear).SetLoops(5, LoopType.Yoyo));
        seq.OnComplete(()=>Destroy(gameObject, 0.5f));
    }

    void OnDisable() {
        DOTween.Kill(this);
    }
}
