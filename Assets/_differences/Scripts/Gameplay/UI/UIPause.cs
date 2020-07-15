using System;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;

using Zenject;

public class UIPause : MonoBehaviour {
    [SerializeField] TextMeshProUGUI _descriptionText = default;

    [Inject] GameplayHandler _handler = default;
    
    UIView _view = default;
    
    const string DESCRIPTION = "It remains to find {0} more differences\nDon't give up!";

    void Awake() {
        _view = GetComponent<UIView>();
    }

    public void OnExitClick() {
        _handler.Exit();
        _view.Hide();
    }

    public void OnContinueClick() {
        _handler.Continue();
        _view.Hide();
    }

    public void Show(int remainDiff) {
        _descriptionText.text = string.Format(DESCRIPTION, remainDiff);
        _view.Show();
    }
}
