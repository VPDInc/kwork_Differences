using _differences.Scripts.NewGeneration.Currency;
using TMPro;

using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIBoosterPreofferElement : MonoBehaviour {
    [SerializeField] TMP_Text _title = default;
    [SerializeField] Image _icon = default;
    [SerializeField] Image _backIcon = default;
    [SerializeField] UICurrencyUpdater _uiCurrencyUpdater = default;

    [Inject] private ICurrencyService currencyService = default;

    UIBoosterShop _uiBoosterShop;
    string _currencyId;

    void Start() {
        _uiBoosterShop = GetComponentInParent<UIBoosterShop>();
    }

    public void Setup(string currencyId, Sprite baseIcon, string title) {
        _title.text = title;
        _currencyId = currencyId;
        _icon.sprite = baseIcon;
        _uiCurrencyUpdater.Setup(currencyId);
    }

    public void SetBackSprite(Sprite back) {
        _backIcon.sprite = back;
    }

    public void OpenShopView() {
        _uiBoosterShop.OpenShopView(_currencyId);
    }
}