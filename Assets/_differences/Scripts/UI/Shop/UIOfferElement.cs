using EasyMobile;

using TMPro;

using UnityEngine;

public class UIOfferElement : MonoBehaviour {
    [SerializeField] TMP_Text _titleText = default;
    [SerializeField] TMP_Text _descriptionText = default;
    [SerializeField] TMP_Text _sellingAmountText = default;
    [SerializeField] TMP_Text _costText = default;

    public void Setup(string title, string description, string buyAmount, string cost) {
        _titleText.text = title;
        _descriptionText.text = description;
        _sellingAmountText.text = buyAmount;
        _costText.text = cost;
    }
}