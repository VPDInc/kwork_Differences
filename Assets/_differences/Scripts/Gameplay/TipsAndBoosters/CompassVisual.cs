using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

public class CompassVisual : MonoBehaviour {
    public void Show() {
        var image = GetComponent<Image>();
        var seq = DOTween.Sequence().SetId(this);
        seq.Append(image.DOFade(0, 0.5f).SetEase(Ease.Linear).SetLoops(5, LoopType.Yoyo));
        seq.OnComplete(()=>Destroy(gameObject, 0.5f));
    }

    void OnDisable() {
        DOTween.Kill(this);
    }
}
