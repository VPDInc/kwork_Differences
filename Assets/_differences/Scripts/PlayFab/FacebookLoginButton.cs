using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

public class FacebookLoginButton : MonoBehaviour {
    [Inject] PlayFabFacebookAuth _playFabFacebookAuth = default;

    public void FacebookLogin() {
        _playFabFacebookAuth.LoginFacebook();
    }

    public void FacebookLink() {
        _playFabFacebookAuth.LinkFacebook();
    }
}