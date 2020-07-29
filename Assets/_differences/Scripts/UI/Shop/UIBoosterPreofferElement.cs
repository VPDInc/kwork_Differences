using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class UIBoosterPreofferElement : MonoBehaviour {
    [SerializeField] TMP_Text _titleLabel = default;
    [SerializeField] TMP_Text _descriptionLabel = default;
    [SerializeField] Image _icon = default;
    [SerializeField] UICurrencyUpdater _uiCurrencyUpdater = default;

    UIBoosterShop _uiBoosterShop;
    string _currencyId;

    void Start() {
        _uiBoosterShop = GetComponentInParent<UIBoosterShop>();
    }

    public void Setup(string currencyId, string title, string description, Sprite baseIcon) {
        _currencyId = currencyId;

        _titleLabel.text = title;
        _descriptionLabel.text = description;
        _icon.sprite = baseIcon;
        _uiCurrencyUpdater.Setup(currencyId);
    }

    public void OpenShopView() {
        _uiBoosterShop.OpenShopView(_currencyId);
    }
}