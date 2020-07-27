using EasyMobile;

using TMPro;

using UnityEngine;

public class UIOfferElement : MonoBehaviour {
    [SerializeField] string _name = default;
    [SerializeField] int _buyAmount = default;
    
    [SerializeField] TMP_Text _titleText = default;
    [SerializeField] TMP_Text _descriptionText = default;
    [SerializeField] TMP_Text _sellingAmountText = default;
    [SerializeField] TMP_Text _costText = default;
    
    IAPProduct _iapProduct;

    public string Name => _name;

    public void Setup(string title, string description, string cost, IAPProduct product) {
        _titleText.text = title;
        _descriptionText.text = description;
        _sellingAmountText.text = _buyAmount + "<sprite=0>";
        _costText.text = cost;
        _iapProduct = product;
    }

    public void Buy() {
        InAppPurchasing.Purchase(_iapProduct);
    }
}