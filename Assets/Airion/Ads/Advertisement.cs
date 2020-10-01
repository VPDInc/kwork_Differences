using System;
using System.Collections.Generic;

using Airion.Extensions;

using UnityEngine;
using UnityEngine.Audio;

public class Advertisement : Singleton<Advertisement> {
    public bool IsAdvertisementEnabled => !_isAdsBought;
    
    [SerializeField] Adapters[] _adapters = default;
    [SerializeField] AdvType _currentAdvType = default;
    
    [Header("Interstitial request settings"),SerializeField] int _startShowingAfterLevel = 2;
    [SerializeField] int _showAfterClicks = 3;
    [SerializeField] float _cooldownAfterShowingAnyAd = 30f;

    bool _isAdsBought = false;
    AdAdapter _currentBannerAd;
    string _bannerPlacement;
    int _interstitialRequestsCount = 0;
    float _lastInterstitialShowingTimestamp;
    
    readonly Dictionary<AdvType, AdAdapter> _adsAdapters = new Dictionary<AdvType, AdAdapter>();

    const string ADS_BOUGHT = "adsBought";

    bool _isAdsDisabled = false;

    public enum AdvType {
        Admob,
    }
    
    [Serializable]
    public struct Adapters {
        public AdAdapter Adapter;
        public AdvType Type;
    }

    protected override void Awake() {
        base.Awake();

        Debug.Assert(_adapters != null && _adapters.Length > 0);
        
        Initialize();
        
        Load();
    }
    
    public void TryShowInterstitial(Action act, string placement, int levelNum) {
        if (_startShowingAfterLevel > levelNum) {
            _interstitialRequestsCount++;
            act?.Invoke();
            return;
        }

        if (_interstitialRequestsCount < _showAfterClicks) {
            _interstitialRequestsCount++;
            act?.Invoke();
            return;
        }

        if (Time.time - _lastInterstitialShowingTimestamp < _cooldownAfterShowingAnyAd) {
            act?.Invoke();
            return;
        }

        ShowInterstitial(val => {
            _lastInterstitialShowingTimestamp = Time.time;
            _interstitialRequestsCount = 0;
        }, completionCallback: act, inGamePlacement:placement);
    }

    void ShowInterstitial(Action<bool> successCallback = null, Action failCallback = null, Action completionCallback = null, string inGamePlacement = "") {
        if (_isAdsBought) {
            successCallback?.Invoke(true);
            completionCallback?.Invoke();
            return;
        }

        if (IsEditor) {
            successCallback?.Invoke(true);
            completionCallback?.Invoke();
            return;
        }

        if (_isAdsDisabled) {
            successCallback?.Invoke(true);
            completionCallback?.Invoke();
            return;
        }

        var ad = GetAdv(_currentAdvType);
        if (!ad.ShowInterstitial((isWatched) => {
            Analytic.Send("video_ads_watch", new Dictionary<string, object>() {
                {"ad_type", "interstitial"},
                {"placement", inGamePlacement},
                {"result", isWatched ? "watched" : "canceled"},
                {"connection", Application.internetReachability != NetworkReachability.NotReachable}
            });
            successCallback?.Invoke(isWatched);
        }, failCallback, completionCallback, () => {
            Analytic.Send("video_ads_started", new Dictionary<string, object>() {
                {"ad_type", "interstitial"},
                {"placement", inGamePlacement},
                {"result", "start"},
                {"connection", Application.internetReachability != NetworkReachability.NotReachable}
            });
        })) {
            Analytic.Send("video_ads_available", new Dictionary<string, object>() {
                {"ad_type", "interstitial"},
                {"placement", inGamePlacement},
                {"result", "not_available"},
                {"connection", Application.internetReachability != NetworkReachability.NotReachable}
            });
            failCallback?.Invoke();
            Err("Can't show interstitial ad");
            return;
        }
        
        Analytic.Send("video_ads_available", new Dictionary<string, object>() {
            {"ad_type", "interstitial"},
            {"placement", inGamePlacement},
            {"result", "success"},
            {"connection", Application.internetReachability != NetworkReachability.NotReachable}
        });
    }
    
    const float LOW_VOLUME = -80;
    const float MAX_VOLUME = 0;
    [SerializeField] AudioMixer _mainMixer = default;


    public void ShowRewardedVideo(Action successCallback = null, Action failCallback = null, Action completionCallback = null, string inGamePlacement = "") {
        if (IsEditor) {
            successCallback?.Invoke();
            completionCallback?.Invoke();
            return;
        }
        
        if (_isAdsDisabled) {
            successCallback?.Invoke();
            completionCallback?.Invoke();
            return;
        }
        
        var ad = GetAdv(_currentAdvType);
        if (!ad.ShowRewardedVideo(() => {
            Analytic.Send("video_ads_watch", new Dictionary<string, object>() {
                {"ad_type", "rewarded"}, 
                {"placement", inGamePlacement},
                {"result", "watched"}, 
                {"connection", Application.internetReachability != NetworkReachability.NotReachable}
            });
            _lastInterstitialShowingTimestamp = Time.time;
            successCallback?.Invoke();
        }, () => {
            Analytic.Send("video_ads_watch", new Dictionary<string, object>() {
                {"ad_type", "rewarded"}, 
                {"placement", inGamePlacement},
                {"result", "canceled"}, 
                {"connection", Application.internetReachability != NetworkReachability.NotReachable}
            });
            failCallback?.Invoke();
        }, ()=> {
            _mainMixer.SetFloat("MasterVolume", MAX_VOLUME);
            completionCallback?.Invoke();
        }, () => {
            _mainMixer.SetFloat("MasterVolume", LOW_VOLUME);

            Analytic.Send("video_ads_started", new Dictionary<string, object>() {
                {"ad_type", "rewarded"},
                {"placement", inGamePlacement},
                {"result", "start"},
                {"connection", Application.internetReachability != NetworkReachability.NotReachable}
            });
        })) {
            Analytic.Send("video_ads_available", new Dictionary<string, object>() {
                {"ad_type", "rewarded"},
                {"placement", inGamePlacement},
                {"result", "not_available"},
                {"connection", Application.internetReachability != NetworkReachability.NotReachable}
            });
            failCallback?.Invoke();
            Err("Cant show rewarded ad");
            return;
        }
        
        Analytic.Send("video_ads_available", new Dictionary<string, object>() {
            {"ad_type", "rewarded"},
            {"placement", inGamePlacement},
            {"result", "success"},
            {"connection", Application.internetReachability != NetworkReachability.NotReachable}
        });
    }

    void OnAdapterInitialized() {
        var ad = GetAdv(_currentAdvType);

        if (IsEditor)
            return;

        if (_isAdsBought)
            return;

        ShowBanner("ads_initialization");
    }

    public void ShowBanner(string inGamePlacement = "") {
        var ad = GetAdv(_currentAdvType);

        if (IsEditor)
          return;

        if (_isAdsBought)
          return;

        _currentBannerAd?.HideBanner();
        _currentBannerAd = ad;
        _currentBannerAd.ShowBanner(OnBannerLoaded, OnBannerFailed, OnBannerClicked);

        _bannerPlacement = inGamePlacement;
        Analytic.Send("video_ads_available", new Dictionary<string, object>() {
            {"ad_type", "banner"},
            {"placement", _bannerPlacement},
            {"result", "success"},
            {"connection", Application.internetReachability != NetworkReachability.NotReachable}
        });
    }
    
    void OnBannerLoaded() {
        Analytic.Send("video_ads_started", new Dictionary<string, object>() {
            {"ad_type", "banner"},
            {"placement",_bannerPlacement},
            {"result", "start"},
            {"connection", Application.internetReachability != NetworkReachability.NotReachable}
        });
        Analytic.Send("video_ads_watch", new Dictionary<string, object>() {
            {"ad_type", "banner"},
            {"placement", _bannerPlacement},
            {"result", "watched"},
            {"connection", Application.internetReachability != NetworkReachability.NotReachable}
        });
    }

    void OnBannerFailed() {        
        Analytic.Send("video_ads_started", new Dictionary<string, object>() {
            {"ad_type", "banner"},
            {"placement",_bannerPlacement},
            {"result", "fail"},
            {"connection", Application.internetReachability != NetworkReachability.NotReachable}
        });
    }

    void OnBannerClicked() {

    }

    public void HideBanner() {
        _currentBannerAd?.HideBanner();
        _currentBannerAd = null;
    }

    public void SwitchOffAdvertisement() {
        _isAdsBought = true;
        SaveAdsState();
        HideBanner();
    }
    
    public void SwitchOnAdvertisement() {
        _isAdsBought = false;
        SaveAdsState();
    }

    void Err(string message) {
        Debug.LogError($"[{GetType()}] {message}");
    }

    void Load() {
        _isAdsBought = PlayerPrefs.GetInt(ADS_BOUGHT, 0) != 0;
    }
    
    void Initialize() {
        foreach (var adapter in _adapters)
            _adsAdapters.Add(adapter.Type, adapter.Adapter);
        
        if (IsEditor)
            return;
        
        GetAdv(_currentAdvType).Initialize(OnAdapterInitialized);
    }

    void SaveAdsState() {
        PlayerPrefs.SetInt(ADS_BOUGHT, _isAdsBought ? 1 : 0);
    }
    
    AdAdapter GetAdv(AdvType type) {
        return _adsAdapters[type];
    }

    bool IsEditor => Application.isEditor;
}
