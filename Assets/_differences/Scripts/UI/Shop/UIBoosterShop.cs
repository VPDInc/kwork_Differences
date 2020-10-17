using System;
using System.Linq;

using Airion.Currency;
using Airion.Extensions;

using Doozy.Engine.UI;

using Sirenix.Utilities;

using TMPro;

using UnityEngine;

using Zenject;

public class UIBoosterShop : Singleton<UIBoosterShop> {
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
    [SerializeField] Sprite[] _backSprites = default;
    [SerializeField] Transform _preofferContainer = default;
    [SerializeField] Transform _offerContainer = default;
    [SerializeField] UIView _shopView = default;
    [SerializeField] TMP_Text _shopTitle = default;
    [SerializeField] Transform _fxStartTransform = default;

    [Inject] CurrencyManager _currencyManager = default;
    [Inject] DiContainer _diContainer = default;

    void Start() {
        SetupPreoffers();
    }

    public static void OpenShop(string boosterCurrency) {
        Instance.OpenShopView(boosterCurrency);
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
        for (var i = 0; i < _shopInfos.Length; i++) {
            BoosterShopInfo shopInfo = _shopInfos[i];
            var preofferElement = _diContainer.InstantiatePrefab(_boosterPreofferElementPrefab, _preofferContainer)
                                              .GetComponent<UIBoosterPreofferElement>();

            preofferElement.Setup(shopInfo.BoosterCurrency, shopInfo.BaseIcon, shopInfo.Title);
            preofferElement.SetBackSprite(_backSprites[(int)Mathf.Repeat(i, _backSprites.Length)]);
        }
    }

    void OpenShopView(BoosterShopInfo shopInfo) {
        _offerContainer.DestroyAllChildren();
        var currency = _currencyManager.GetCurrency(shopInfo.BoosterCurrency);
        _shopTitle.text = shopInfo.Description;
        for (var i = 0; i < shopInfo.Boosters.Length; i++) {
            BoosterInfo boosterInfo = shopInfo.Boosters[i];
            var offerElement = _diContainer.InstantiatePrefab(_boosterOfferElementPrefab, _offerContainer)
                                           .GetComponent<UIBoosterOfferElement>();

            offerElement.SetBackSprite(_backSprites[(int) Mathf.Repeat(i, _backSprites.Length)]);
            var title = boosterInfo.Title.IsNullOrWhitespace() ? shopInfo.Title : boosterInfo.Title;
            var icon = boosterInfo.Icon ? boosterInfo.Icon : shopInfo.BaseIcon;
            print(title);
            offerElement.Setup(currency, title, icon, boosterInfo.Amount, boosterInfo.Cost, _fxStartTransform);
        }

        _shopView.Show();
    }
}