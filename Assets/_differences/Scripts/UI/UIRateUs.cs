using Airion.Extensions;

using DG.Tweening;

using EasyMobile;

using UnityEngine;

public class UIRateUs : MonoBehaviour {
    [SerializeField] CanvasGroup _group = default;

    bool IsRated {
        get => PrefsExtensions.GetBool(IS_RATED_PREFS, false);
        set => PrefsExtensions.SetBool(IS_RATED_PREFS, value);
    }

    const string IS_RATED_PREFS = "is_rated";

    void Awake() {
        if (IsRated)
            HideGroup(true);
    }

    public void OnPositiveRated() {
        IsRated = true;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Application.OpenURL("itms-apps://itunes.apple.com/app/id" + EM_Settings.RatingRequest.IosAppId + "?action=write-review");
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_5_6_OR_NEWER
            Application.OpenURL("market://details?id=" + Application.identifier);
#else
            Application.OpenURL("market://details?id=" + Application.bundleIdentifier);
#endif
        }
        HideGroup();
    }

    public void OnNegativeRated() {
        IsRated = true;
        Application.OpenURL("mailto:" + EM_Settings.RatingRequest.SupportEmail);
        HideGroup();
    }

    void HideGroup(bool fast = false) {
        _group.DOKill();
        
        if (fast) {
            _group.alpha = 0;
            return;
        }

        _group.DOFade(0, 1);
    }

}
