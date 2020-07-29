using System;
using System.Linq;

using Airion.Currency;
using Airion.Extensions;

using Doozy.Engine.UI;

using Sirenix.Utilities;

using UnityEngine;

using Zenject;

public class UIBoosterShop : MonoBehaviour {
    [Serializable]
    class BoosterShopInfo {
        public string Title = default;
        public string Description = default;
        public string BoosterCurrency = default;
        public Sprite BaseIcon = default;
        public BoosterInfo[] Boosters = default;
    }

    [Serializable]
    class BoosterInfo {
        public string Title = default;
        public int Amount = default;
        public int Cost = default;
        public Sprite Icon = default;
    }

    [SerializeField] UIBoosterOfferElement _boosterOfferElementPrefab = default;
    [SerializeField] UIBoosterPreofferElement _boosterPreofferElementPrefab = default;
    [SerializeField] BoosterShopInfo[] _shopInfos = default;
    [SerializeField] Transform _preofferContainer = default;
    [SerializeField] Transform _offerContainer = default;
    [SerializeField] UIView _shopView = default;

    [Inject] CurrencyManager _currencyManager = default;
    [Inject] DiContainer _diContainer = default;

    void Start() {
        SetupPreoffers();
    }

    public void OpenShopView(string boosterCurrency) {
        var shopInfo = _shopInfos.FirstOrDefault(x => x.BoosterCurrency.Equals(boosterCurrency));
        if (shopInfo == null) {
            Debug.LogError("Can't open shop for " + boosterCurrency);
        } else {
            OpenShopView(shopInfo);
        }
    }

    void SetupPreoffers() {
        _preofferContainer.DestroyAllChildren();
        foreach (BoosterShopInfo shopInfo in _shopInfos) {
            var preofferElement = _diContainer.InstantiatePrefab(_boosterPreofferElementPrefab, _preofferContainer).GetComponent<UIBoosterPreofferElement>();
            preofferElement.Setup(shopInfo.BoosterCurrency, shopInfo.Title, shopInfo.Description, shopInfo.BaseIcon);
        }
    }

    void OpenShopView(BoosterShopInfo shopInfo) {
        _offerContainer.DestroyAllChildren();
        var currency = _currencyManager.GetCurrency(shopInfo.BoosterCurrency);
        foreach (BoosterInfo boosterInfo in shopInfo.Boosters) {
            var offerElement = _diContainer.InstantiatePrefab(_boosterOfferElementPrefab, _offerContainer).GetComponent<UIBoosterOfferElement>();
            var title = boosterInfo.Title.IsNullOrWhitespace() ? shopInfo.Title : boosterInfo.Title;
            var icon = boosterInfo.Icon ? boosterInfo.Icon : shopInfo.BaseIcon;
            offerElement.Setup(currency, title, icon, boosterInfo.Amount, boosterInfo.Cost);
        }
        
        _shopView.Show();
    }
}