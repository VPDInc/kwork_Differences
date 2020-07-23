using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class UIOpenLinkButton : MonoBehaviour {
    [SerializeField] string _url = default;

    public void OpenUrl() {
        Application.OpenURL(_url);
    }
}