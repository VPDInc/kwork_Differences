using System;

using UnityEngine;
using UnityEngine.Serialization;

// public class ApplovinAdAdapter : AdAdapter {
    // [SerializeField] string _sdkKey = default;
    // [FormerlySerializedAs("_interstitialAdUnitId")] [SerializeField] string _androidInterstitialAdUnitId = default;
    // [FormerlySerializedAs("_rewardedAdUnitId")] [SerializeField] string _androidRewardedAdUnitId = default;
    // [FormerlySerializedAs("_bannerAdUnitId")] [SerializeField] string _androidBannerAdUnitId = default; //
    // [SerializeField] string _iosInterstitialAdUnitId = default;
    // [SerializeField] string _iosRewardedAdUnitId = default;
    // [SerializeField] string _iosBannerAdUnitId = default; // 
    // [SerializeField] Color _bannerBackgroundColor = Color.white;
    // [SerializeField] bool _isDebugEnabled = false;
    //
    // string _interstitialAdUnitId;
    // string _rewardedAdUnitId;
    // string _bannerAdUnitId;
    // int _interstitialRetryAttempt = 0;
    // int _rewardedRetryAttempt = 0;
    //
    // Action _successRewarded;
    // Action _failRewarded;
    // Action _completeRewarded;
    //
    // Action<bool> _successInterstitial;
    // Action _failInterstitial;
    // Action _completeInterstitial;
    // Action _initCallback;
    // Action _videoStartedCallback;
    //
    // Action _bannerLoaded;
    // Action _bannerLoadFailed;
    // Action _bannerClicked;
    //
    // #region AdAdapter
    //
    // public override void Initialize(Action initCallback) {
    //     _initCallback = initCallback;
    //     if (Application.platform == RuntimePlatform.Android) {
    //         _interstitialAdUnitId = _androidInterstitialAdUnitId;
    //         _rewardedAdUnitId = _androidRewardedAdUnitId;
    //         _bannerAdUnitId = _androidBannerAdUnitId;
    //     } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
    //         _interstitialAdUnitId = _iosInterstitialAdUnitId;
    //         _rewardedAdUnitId = _iosRewardedAdUnitId;
    //         _bannerAdUnitId = _iosBannerAdUnitId;
    //     }
    //     
    //     MaxSdkCallbacks.OnSdkInitializedEvent += OnInitialized;
    //     
    //     MaxSdk.SetSdkKey(_sdkKey);
    //     MaxSdk.InitializeSdk();
    //     
    //     InitializeInterstitialAds();
    //     InitializeRewardedAds();
    //     InitializeBannerAds();
    // }
    //
    // public override bool ShowInterstitial(Action<bool> successCallback = null, Action failCallback = null,
    //     Action completionCallback = null,
    //     Action videoStartedCallback = null, string segment = null) {
    //     
    //     _successInterstitial = successCallback;
    //     _failInterstitial = failCallback;
    //     _completeInterstitial = completionCallback;
    //     _videoStartedCallback = videoStartedCallback;
    //     
    //     if (MaxSdk.IsInterstitialReady(_interstitialAdUnitId)) {
    //         MaxSdk.ShowInterstitial(_interstitialAdUnitId);
    //         Log("Interstitial is ready");
    //         return true;
    //     }
    //
    //     _completeInterstitial?.Invoke();
    //     _failInterstitial?.Invoke();
    //     Log("Interstitial is not ready");
    //     return false;
    // }
    //
    // public override bool ShowRewardedVideo(Action successCallback = null, Action failCallback = null,
    //     Action completionCallback = null,
    //     Action videoStartedCallback = null, string segment = null) {
    //     _successRewarded = successCallback;
    //     _failRewarded = failCallback;
    //     _completeRewarded = completionCallback;
    //     _videoStartedCallback = videoStartedCallback;
    //     
    //     if (MaxSdk.IsRewardedAdReady(_rewardedAdUnitId)) {
    //         MaxSdk.ShowRewardedAd(_rewardedAdUnitId);
    //         Log("Rewarded is ready");
    //         return true;
    //     }
    //
    //     Log("Rewarded is not ready");
    //     _completeRewarded?.Invoke();
    //     _failRewarded?.Invoke();
    //     return false;
    // }
    //
    // public override bool ShowBanner(Action bannerLoaded = null, Action bannerLoadFailed = null, Action bannerClicked = null, string segment = null) {
    //     _bannerLoaded = bannerLoaded;
    //     _bannerClicked = bannerClicked;
    //     _bannerLoadFailed = bannerLoadFailed;
    //     
    //     MaxSdk.ShowBanner(_bannerAdUnitId);
    //     return true;
    // }
    //
    // public override bool HideBanner() {
    //     MaxSdk.HideBanner(_bannerAdUnitId);
    //     return true;
    // }
    //
    // #endregion // AdAdapter
    //
    // #region Applovin
    //
    // #region Banner
    //
    // void InitializeBannerAds() {
    //     // Banners are automatically sized to 320x50 on phones and 728x90 on tablets
    //     // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments
    //     MaxSdkCallbacks.OnBannerAdClickedEvent += OnBannerClicked;
    //     MaxSdkCallbacks.OnBannerAdLoadedEvent += OnBannerLoaded;
    //     MaxSdkCallbacks.OnBannerAdLoadFailedEvent += OnBannerLoadFailed;
    //     
    //     MaxSdk.CreateBanner(_bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
    //
    //     // Set background or background color for banners to be fully functional
    //     MaxSdk.SetBannerBackgroundColor(_bannerAdUnitId, _bannerBackgroundColor);
    //
    //     Log("Banner initialized");
    // }
    //
    // void OnBannerLoadFailed(string arg1, int arg2) {
    //     _bannerLoadFailed?.Invoke();
    // }
    //
    // void OnBannerLoaded(string obj) {
    //     _bannerLoaded?.Invoke();   
    // }
    //
    // void OnBannerClicked(string obj) {
    //     _bannerClicked?.Invoke();
    // }
    //
    // #endregion // Banner
    //
    // #region Initialize
    //
    // void OnInitialized(MaxSdkBase.SdkConfiguration configuration) {
    //     Log("MaxSDK initialized: " + configuration.ConsentDialogState);
    //     if (_isDebugEnabled)
    //         MaxSdk.ShowMediationDebugger();
    //     _initCallback?.Invoke();
    // }
    //
    // #endregion // Initialize
    //
    // #region Rewarded
    //
    // void InitializeRewardedAds() {
    //     // Attach callback
    //     MaxSdkCallbacks.OnRewardedAdLoadedEvent += OnRewardedAdLoadedEvent;
    //     MaxSdkCallbacks.OnRewardedAdLoadFailedEvent += OnRewardedAdFailedEvent;
    //     MaxSdkCallbacks.OnRewardedAdFailedToDisplayEvent += OnRewardedAdFailedToDisplayEvent;
    //     MaxSdkCallbacks.OnRewardedAdDisplayedEvent += OnRewardedAdDisplayedEvent;
    //     MaxSdkCallbacks.OnRewardedAdClickedEvent += OnRewardedAdClickedEvent;
    //     MaxSdkCallbacks.OnRewardedAdHiddenEvent += OnRewardedAdDismissedEvent;
    //     MaxSdkCallbacks.OnRewardedAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
    //
    //     // Load the first RewardedAd
    //     LoadRewardedAd();
    // }
    //
    // void LoadRewardedAd() {
    //     MaxSdk.LoadRewardedAd(_rewardedAdUnitId);
    //     Log("Try to load Rewarded");
    // }
    //
    // void OnRewardedAdLoadedEvent(string adUnitId) {
    //     // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'
    //     Log("Rewarded loaded");
    //     // Reset retry attempt
    //     _rewardedRetryAttempt = 0;
    // }
    //
    // void OnRewardedAdFailedEvent(string adUnitId, int errorCode) {
    //     // Rewarded ad failed to load. We recommend retrying with exponentially higher delays.
    //     Log("Fail to load Rewarded");
    //     _rewardedRetryAttempt++;
    //     double retryDelay = Math.Pow(2, _rewardedRetryAttempt);
    //     _failRewarded?.Invoke();
    //     Invoke(nameof(LoadRewardedAd), (float) retryDelay);
    // }
    //
    // void OnRewardedAdFailedToDisplayEvent(string adUnitId, int errorCode) {
    //     // Rewarded ad failed to display. We recommend loading the next ad
    //     Log("Failed to display Rewarded");
    //     _failRewarded?.Invoke();
    //     _completeRewarded?.Invoke();
    //     LoadRewardedAd();
    // }
    //
    // void OnRewardedAdDisplayedEvent(string adUnitId) {
    //     Log("Rewarded started");
    //     _videoStartedCallback?.Invoke();
    // }
    //
    // void OnRewardedAdClickedEvent(string adUnitId) {
    //     Log("Rewarded clicked");
    // }
    //
    // void OnRewardedAdDismissedEvent(string adUnitId) {
    //     // Rewarded ad is hidden. Pre-load the next ad
    //     _failRewarded?.Invoke();
    //     _completeRewarded?.Invoke();
    //     LoadRewardedAd();
    // }
    //
    // void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward) {
    //     _successRewarded?.Invoke();
    //     _completeRewarded?.Invoke();
    //     // Rewarded ad was displayed and user should receive the reward
    // }
    //
    // #endregion // Rewarded
    //
    // #region Interstitial
    //
    // void InitializeInterstitialAds() {
    //     // Attach callback
    //     MaxSdkCallbacks.OnInterstitialLoadedEvent += OnInterstitialLoadedEvent;
    //     MaxSdkCallbacks.OnInterstitialDisplayedEvent += OnInterstitialDisplayedEvent;
    //     MaxSdkCallbacks.OnInterstitialLoadFailedEvent += OnInterstitialFailedEvent;
    //     MaxSdkCallbacks.OnInterstitialAdFailedToDisplayEvent += InterstitialFailedToDisplayEvent;
    //     MaxSdkCallbacks.OnInterstitialHiddenEvent += OnInterstitialDismissedEvent;
    //     MaxSdkCallbacks.OnInterstitialClickedEvent += OnInterstitialClickedEvent;
    //
    //     // Load the first interstitial
    //     LoadInterstitial();
    // }
    //
    // void OnInterstitialClickedEvent(string obj) {
    //     Log("Interstitial clicked");
    // }
    //
    // void LoadInterstitial() {
    //     MaxSdk.LoadInterstitial(_interstitialAdUnitId);
    // }
    //
    // void OnInterstitialDisplayedEvent(string unitId) {
    //     Log("Interstitial started");
    //     _videoStartedCallback?.Invoke();
    // }
    //
    // void OnInterstitialLoadedEvent(string adUnitId) {
    //     // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(_interstitialAdUnitId) will now return 'true'
    //     Log("Interstitial loaded");
    //     // Reset retry attempt
    //     _interstitialRetryAttempt = 0;
    // }
    //
    // void OnInterstitialFailedEvent(string adUnitId, int errorCode) {
    //     // Interstitial ad failed to load. We recommend retrying with exponentially higher delays.
    //     Log("Interstitial failed to load");
    //     _interstitialRetryAttempt++;
    //     double retryDelay = Math.Pow(2, _interstitialRetryAttempt);
    //
    //     Invoke(nameof(LoadInterstitial), (float) retryDelay);
    // }
    //
    // void InterstitialFailedToDisplayEvent(string adUnitId, int errorCode) {
    //     // Interstitial ad failed to display. We recommend loading the next ad
    //     _failInterstitial?.Invoke();
    //     _completeInterstitial?.Invoke();
    //     LoadInterstitial();
    // }
    //
    // void OnInterstitialDismissedEvent(string adUnitId) {
    //     // Interstitial ad is hidden. Pre-load the next ad
    //     _successInterstitial?.Invoke(true);
    //     _completeInterstitial?.Invoke();
    //     LoadInterstitial();
    // }
    //
    // #endregion // Interstitial
    //
    // #endregion // Applovin
    //
    // #region Helpers
    //
    // void Log(string message) {
    //     if (_isDebugEnabled)
    //         Debug.Log($"[{GetType()}] {message}");
    // }
    //
    // void Err(string message) {
    //     Debug.LogError($"[{GetType()}] {message}");
    // }
    //
    // #endregion // Helpers
// }
