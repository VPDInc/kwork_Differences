using System;

using GoogleMobileAds.Api;

// using admob;

using UnityEngine;

public class AdmobAdAdapter : AdAdapter {
    [SerializeField] string _iosBannerID="ca-app-pub-3940256099942544/2934735716";
    [SerializeField] string _iosInterstitialID="ca-app-pub-3940256099942544/4411468910";
    [SerializeField] string _iosRewardedVideoID="ca-app-pub-3940256099942544/1712485313";
    [SerializeField] string _iosNativeBannerID = "ca-app-pub-3940256099942544/3986624511";
        
    [SerializeField] string _androidBannerID="ca-app-pub-3940256099942544/6300978111";
    [SerializeField] string _androidInterstitialID="ca-app-pub-3940256099942544/1033173712";
    [SerializeField] string _androidRewardedVideoID="ca-app-pub-3940256099942544/5224354917";
    [SerializeField] string _androidNativeBannerID = "ca-app-pub-3940256099942544/2247696110";
    
    [SerializeField] bool _isDebugEnabled = false;

    bool _isShowedRewarded = false;
    
    string _bannerID="";
    string _interstitialID="";
    string _rewardedVideoID="";
    string _nativeBannerID = "";

    RewardedAd _rewardedAd;

    Action _successRewarded;
    Action _failRewarded;
    Action _completeRewarded;
    
    Action<bool> _successInterstitial;
    Action _failInterstitial;
    Action _completeInterstitial;
    Action _initCallback;
    Action _videoStartedCallback;
    
    Action _bannerLoaded;
    Action _bannerLoadFailed;
    Action _bannerClicked;
    
    #region AdAdapter
    
    public override void Initialize(Action initCallback) {
        if (Application.platform == RuntimePlatform.Android) {
            _bannerID = _androidBannerID;
            _interstitialID = _androidInterstitialID;
            _rewardedVideoID = _androidRewardedVideoID;
            _nativeBannerID = _androidNativeBannerID;
        } else {
            _bannerID = _iosBannerID;
            _interstitialID = _iosInterstitialID;
            _rewardedVideoID = _iosRewardedVideoID;
            _nativeBannerID = _iosNativeBannerID;
        }

        MobileAds.Initialize(status => {
            Log($"initialized with status {status}");
        });
        
        _rewardedAd = CreateAndLoad();
    }

    RewardedAd CreateAndLoad() {
        var rewardedAd = new RewardedAd(_rewardedVideoID);
        
        AdRequest request = new AdRequest.Builder().Build();
        rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        rewardedAd.OnAdClosed += HandleRewardedAdClosed;
        rewardedAd.LoadAd(request);
        // Called when an ad request has successfully loaded.
        // _rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        // _rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        // _rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        // _rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        // Called when the ad is closed.
        return rewardedAd;
    }

    void HandleRewardedAdClosed(object sender, EventArgs e) {
        Log("Handled close reward");
        _rewardedAd = CreateAndLoad();
    }

    void HandleUserEarnedReward(object sender, Reward e) {
        Log("Handled earn reward");
        _isShowedRewarded = true;
    }

    // void OnNativeBannerEvent(string eventname, string msg) {
    // }
    //
    // void OnRewardedVideoEvent(string eventname, string msg) {
    //     Log($"{eventname} {msg}");
    //     if (eventname == AdmobEvent.onRewarded) {
    //         _successRewarded?.Invoke();
    //         Admob.Instance().loadRewardedVideo(_rewardedVideoID);
    //         _completeRewarded?.Invoke();
    //     }
    // }

    // void OnInterstitialEvent(string eventname, string msg) {
    //     Log($"{eventname} {msg}");
    //     if (eventname == AdmobEvent.onRewarded) {
    //         _successInterstitial?.Invoke(true);
    //         Admob.Instance().loadInterstitial(_interstitialID);
    //         _completeInterstitial?.Invoke();
    //     }
    // }
    //
    // void OnBannerEvent(string eventname, string msg) {
    // }


    public override bool ShowInterstitial(Action<bool> successCallback = null, Action failCallback = null,
        Action completionCallback = null,
        Action videoStartedCallback = null, string segment = null) {
        
        _successInterstitial = successCallback;
        _failInterstitial = failCallback;
        _completeInterstitial = completionCallback;
        _videoStartedCallback = videoStartedCallback;
        
        // if (Admob.Instance().isInterstitialReady()) {
        //     Admob.Instance().showInterstitial();
        //     Log("Interstitial is ready");
        //     return true;
        // }
    
        _failInterstitial?.Invoke();
        _completeInterstitial?.Invoke();
        Log("Interstitial is not ready");
        return false;
    }
    
    public override bool ShowRewardedVideo(Action successCallback = null, Action failCallback = null,
        Action completionCallback = null,
        Action videoStartedCallback = null, string segment = null) {
        _successRewarded = successCallback;
        _failRewarded = failCallback;
        _completeRewarded = completionCallback;
        _videoStartedCallback = videoStartedCallback;
        
        if (_rewardedAd.IsLoaded()) {
            _rewardedAd.Show();
            Log("Rewarded is ready");
            return true;
        }

        Log("Rewarded is not ready");
        _failRewarded?.Invoke();
        _completeRewarded?.Invoke();
        return false;
    }

    void Update() {
        if (_isShowedRewarded) {
            _isShowedRewarded = false;
            _successRewarded?.Invoke();
            _completeRewarded?.Invoke();
        }
    }

    public override bool ShowBanner(Action bannerLoaded = null, Action bannerLoadFailed = null, Action bannerClicked = null, string segment = null) {
        _bannerLoaded = bannerLoaded;
        _bannerClicked = bannerClicked;
        _bannerLoadFailed = bannerLoadFailed;
        // Admob.Instance().showBannerRelative(_bannerID, AdSize.SMART_BANNER, AdPosition.BOTTOM_CENTER);
        return true;
    }
    
    public override bool HideBanner() {
        // Admob.Instance().removeBanner();
        return true;
    }
    
    #endregion // AdAdapter
    
    
    #region Helpers
    
    void Log(string message) {
        if (_isDebugEnabled)
            Debug.Log($"[{GetType()}] {message}");
    }
    
    void Err(string message) {
        Debug.LogError($"[{GetType()}] {message}");
    }
    
    #endregion // Helpers
}
