using UnityEngine;
using UnityEngine.UI;

public class UIBoosterPreofferElement : MonoBehaviour {
    [SerializeField] Image _icon = default;
    [SerializeField] Image _backIcon = default;
    [SerializeField] UICurrencyUpdater _uiCurrencyUpdater = default;

    UIBoosterShop _uiBoosterShop;
    string _currencyId;

    void Start() {
        _uiBoosterShop = GetComponentInParent<UIBoosterShop>();
    }

    public void Setup(string currencyId, Sprite baseIcon) {
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