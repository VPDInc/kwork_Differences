using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

public class FacebookButton : MonoBehaviour {
    [Inject] PlayFabFacebook _playFabFacebook = default;

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