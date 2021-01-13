using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

public class FacebookButton : MonoBehaviour {
    [Inject] PlayFabFacebook _playFabFacebook = default;
    [SerializeField] private CollectFx collectFx = default;
    [SerializeField] private bool enableFx = false;

    private void Start()
    {
        _playFabFacebook.FacebookLinked += OnFacebookLinked;
    }

    private void OnDestroy()
    {
        _playFabFacebook.FacebookLinked -= OnFacebookLinked;
    }

    private void OnFacebookLinked()
    {
        if(enableFx)
        {
            collectFx.SetupTrailEffect(delegate { _playFabFacebook.SetReward(); });
        }
        else
        {
            _playFabFacebook.SetReward();
        }
    }

    public void FacebookLogin() {
        _playFabFacebook.LoginFacebook();
    }

    public void FacebookLink() {
        _playFabFacebook.LinkFacebook();
    }

    public void FacebookUnlink() {
        _playFabFacebook.UnlinkFacebook();
    }
}