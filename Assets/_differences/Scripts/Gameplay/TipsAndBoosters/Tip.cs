using Airion.Currency;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public abstract class Tip : MonoBehaviour {
   [SerializeField] string _currencyId = default;
   [SerializeField] TextMeshProUGUI _amountText = default;

   [Inject] CurrencyManager _currencyManager = default;
   
   Button _button;
   Currency _currency;
   
   void Awake() {
      _button = GetComponentInChildren<Button>();
   }

   protected virtual void Start() {
      _currency = _currencyManager.GetCurrency(_currencyId);
      _currency.Updated += OnCurrencyUpdated;
      UpdateVisual();
   }

   protected virtual void OnDestroy() {
      _currency.Updated -= OnCurrencyUpdated;
   }
   
   public virtual void OnButtonClick() {
      _currency.Spend(1);
   }

   void SetInteractable(bool isInteractable) {
      _button.interactable = isInteractable;
   }

   void UpdateVisual() {
      SetInteractable(_currency.IsEnough(1));
      _amountText.text = _currency.Amount.ToString("F0");
   }
   
   void OnCurrencyUpdated(float obj) {
      UpdateVisual();
   }
}
