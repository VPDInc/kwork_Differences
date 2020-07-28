using System;

#if AIRION_UNITY_ADS_MEDIATOR

using UnityEngine;
using UnityEngine.Advertisements;

public class UnityAdAdapter : AdAdapter, IUnityAdsListener {
    #pragma warning disable 0414
    [SerializeField] string _iosKey = default;
    #pragma warning restore 0414
    [SerializeField] string _androidKey = default;
    [SerializeField] bool _isTestMode = true;
    [SerializeField] bool _isDebugEnabled = false;
    [SerializeField] string _rewardedVideoPlacement = "revardedVideo";
    [SerializeField] string _interstitialPlacement = "video";

    Action _successRewarded;
    Action _failRewarded;
    Action _completeRewarded;
    
    Action<bool> _successIntersitital;
    Action _failIntersitital;
    Action _completeIntersitital;

    Action _rewardStartedCallback;

    public override bool ShowInterstitial(Action<bool> successCallback = null, Action failCallback = null, Action completionCallback = null, Action rewardStartedCallback = null, string segment = null) {
        _successIntersitital = successCallback;
        _failIntersitital = failCallback;
        _completeIntersitital = completionCallback;
        _rewardStartedCallback = rewardStartedCallback;
        
        if (!UnityEngine.Advertisements.Advertisement.IsReady(_interstitialPlacement)) {
            Log("Interstitial is not ready");
            _completeIntersitital?.Invoke();
            _failIntersitital?.Invoke();
            return false;
        }
        
        UnityEngine.Advertisements.Advertisement.Show(_interstitialPlacement);
        Log("Interstitial is ready");

        return true;
    }

    public override bool ShowRewardedVideo(Action successCallback = null, Action failCallback = null, Action completionCallback = null, Action rewardStartedCallback = null, string segment = null) {
        _successRewarded = successCallback;
        _failRewarded = failCallback;
        _completeRewarded = completionCallback;
        _rewardStartedCallback = rewardStartedCallback;
        
        if (!UnityEngine.Advertisements.Advertisement.IsReady(_rewardedVideoPlacement)) {
            Log("Rewarded is not ready");
            _completeRewarded?.Invoke();
            _failRewarded?.Invoke();
            return false;
        }
        
        UnityEngine.Advertisements.Advertisement.Show(_rewardedVideoPlacement);
        Log("Rewarded is ready");

        return true;
    }

    public override bool ShowBanner(string segment = null) {
        Log("Banner not defined");
        return false;
    }

    public override bool HideBanner() {
        Log("Banner not defined");
        return false;
    }

    public override void Initialize() {
        #if UNITY_ANDROID
        var key = _androidKey;
        #else
        var key = _iosKey;
        #endif

        Log("Initialized");
        
        UnityEngine.Advertisements.Advertisement.AddListener(this);
        UnityEngine.Advertisements.Advertisement.Initialize(key, _isTestMode);
    }

    public void OnUnityAdsReady(string placementId) {
        Log(nameof(OnUnityAdsReady) + placementId);
    }

    public void OnUnityAdsDidError(string message) {
        Err(nameof(OnUnityAdsDidError) + " " + message);
    }

    public void OnUnityAdsDidStart(string placementId) {
        Log(nameof(OnUnityAdsDidStart));
        _rewardStartedCallback?.Invoke();
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult) {
        Log(nameof(OnUnityAdsDidFinish));

        if (placementId == _interstitialPlacement) {
            switch (showResult) {
                case ShowResult.Failed:
                    _failIntersitital?.Invoke();
                    break;
                case ShowResult.Skipped:
                    _successIntersitital?.Invoke(false);
                    break;
                case ShowResult.Finished:
                    _successIntersitital?.Invoke(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(showResult), showResult, null);
            }
            _completeIntersitital?.Invoke();
            return;
        }
        
        if (placementId == _rewardedVideoPlacement) {
            switch (showResult) {
                case ShowResult.Failed:
                    _failRewarded?.Invoke();
                    break;
                case ShowResult.Skipped:
                    _successRewarded?.Invoke();
                    break;
                case ShowResult.Finished:
                    _successRewarded?.Invoke();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(showResult), showResult, null);
            }
            _completeRewarded?.Invoke();
            return;
        }
    }

    void Log(string message) {
        if (_isDebugEnabled)
            Debug.Log($"[{GetType()}] {message}");
    }
    
    void Err(string message) {
        Debug.LogError($"[{GetType()}] {message}");
    }
}

#endif // AIRION_UNITY_ADS_MEDIATOR
