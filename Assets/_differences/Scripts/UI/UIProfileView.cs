using System.Collections;
using System.Collections.Generic;

using Doozy.Engine.UI;

using UnityEngine;

public class UIProfileView : MonoBehaviour {
    UIView _currentView = default;

    void Awake() {
        _currentView = GetComponent<UIView>();
    }

    public void Show(bool instant) {
        _currentView.Show(instant);
    }

    public void Hide(bool instant) {
        _currentView.Hide(instant);
    }
}