using System.Collections;
using System.Collections.Generic;

using Doozy.Engine.UI;

using UnityEngine;

public class UIProfileView : MonoBehaviour {
    UIView _currentView = default;

    void Awake() {
        _currentView = GetComponent<UIView>();
    }

    public void Show() {
        _currentView.Show();
    }

    public void Hide() {
        _currentView.Hide();
    }
}