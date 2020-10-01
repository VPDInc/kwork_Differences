using System;

using UnityEngine;

public abstract class AdAdapter : MonoBehaviour { 
    public abstract bool ShowInterstitial(Action<bool> successCallback = null, Action failCallback = null, Action completionCallback = null, Action videoStartedCallback = null, string segment = null);
    public abstract bool ShowRewardedVideo(Action successCallback = null, Action failCallback = null, Action completionCallback = null,Action videoStartedCallback = null, string segment = null);
    public abstract bool ShowBanner(Action bannerLoaded = null, Action bannerLoadFailed = null, Action bannerClicked = null, string segment = null);
    public abstract bool HideBanner();
    public abstract void Initialize(Action initCallback);
}
