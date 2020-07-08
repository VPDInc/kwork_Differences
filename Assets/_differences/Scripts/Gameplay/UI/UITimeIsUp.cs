using System;

using Airion.Currency;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UITimeIsUp : MonoBehaviour {
    [SerializeField] Button _addTimeButton = default;
    [SerializeField] int _addTimeCost = 900;
    [SerializeField] GameObject _normalGroup = default;
    [SerializeField] GameObject _warnGroup = default;
    [SerializeField] TextMeshProUGUI _description = default;
    [SerializeField] TextMeshProUGUI _competitionPoints = default;
    [SerializeField] TextMeshProUGUI _addTimeText = default;
    
    [Inject] CurrencyManager _currencyManager = default;
    [Inject] GameplayHandler _handler = default;

    Currency _soft = default;
    UIView _view = default;
    bool _isWarn = false;

    const string PLAY_BUTTON_FORMAT = "Play <sprite=0> {0}";
    const string COMPETITION_POINTS_FORMAT = "X {0}";
    const string ADD_TIME_FORMAT = "+{0} S";
    const string WARN_DESCRIPTION = "If you give up now, you'll lose competition points";
    const string NORMAL_DESCRIPTION = "Buy some extra time to keep playing";

    void Start() {
        _soft = _currencyManager.GetCurrency("Soft");        
        _view = GetComponent<UIView>();
    }

    public void Show(float timeToAdd, int ratingWillBeLost) {
        _view.Show();
        _addTimeButton.interactable = IsEnoughToAddTime;
        _isWarn = false;
        Fill(timeToAdd, ratingWillBeLost);
        SwitchToWarn(false);
    }
    
    bool IsEnoughToAddTime => _soft.IsEnough(_addTimeCost);

    void Fill(float timeToAdd, int ratingWillBeLost) {
        _addTimeButton.GetComponentInChildren<TextMeshProUGUI>().text = string.Format(PLAY_BUTTON_FORMAT, _addTimeCost);
        _competitionPoints.text = string.Format(COMPETITION_POINTS_FORMAT, ratingWillBeLost);
        _addTimeText.text = string.Format(ADD_TIME_FORMAT, timeToAdd);
    }
    
    void SwitchToWarn(bool isWarn) {
        _description.text = isWarn ? WARN_DESCRIPTION : NORMAL_DESCRIPTION;
        _warnGroup.SetActive(isWarn);
        _normalGroup.SetActive(!isWarn);
    }

    public void OnAddTimeClick() {
        _soft.Spend(_addTimeCost);
        _handler.Continue();
        _view.Hide();
    }

    public void OnExitClick() {
        if (!_isWarn) {
            if (IsEnoughToAddTime) {
                SwitchToWarn(true);
                _isWarn = true;
                return;
            }
        }
        
        _view.Hide();
        _handler.Exit();
    }
}
