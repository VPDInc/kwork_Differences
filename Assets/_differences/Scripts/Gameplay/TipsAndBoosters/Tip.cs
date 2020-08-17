using Airion.Currency;

using Doozy.Engine;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public abstract class Tip : MonoBehaviour {
   [SerializeField] string _currencyId = default;
   [SerializeField] TextMeshProUGUI _amountText = default;
   [SerializeField] GameObject _plusGroup = default;
   [SerializeField] GameObject _amountGroup = default;
   [SerializeField] bool _alwaysOpenStore = default;

   [Inject] CurrencyManager _currencyManager = default;
   [Inject] GameplayHandler _gameplayHandler = default;
    
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
   
   public void OnButtonClick() {
      if (_alwaysOpenStore) {
         OpenStore();
         return;
      }

      if (_currency.IsEnough(1)) {
         if (TryActivate()) {
            _currency.Spend(1);
         }
      } else {
         _gameplayHandler.Pause();
         OpenStore();
      }
   }

   void OpenStore() {
      UIBoosterShop.OpenShop(_currencyId);
   }

   protected abstract bool TryActivate();

   void EnableGroup(bool isEnough) {
      _plusGroup.SetActive(!isEnough);
      _amountGroup.SetActive(isEnough);
   }

   void UpdateVisual() {
      EnableGroup(_currency.IsEnough(1));
      _amountText.text = _currency.Amount.ToString("F0");
   }
   
   void OnCurrencyUpdated(float obj) {
      UpdateVisual();
   }
}
