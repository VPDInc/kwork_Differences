using System;
using System.Linq;

using Airion.Currency;

using EasyMobile;

using GameAnalyticsSDK;

using UnityEngine;
using UnityEngine.Purchasing;

using Zenject;

public class UICoinShopView : MonoBehaviour {
    [SerializeField] OfferInfo[] _offerInfos = default;

    [Inject] CurrencyManager _currencyManager = default;

    Currency _softCurrency;

    const string SOFT_CURRENCY_ID = "Soft";

    [Serializable]
    class OfferInfo {
        public string Name = default;
        public int CoinsAmount = default;
        public UIOfferElement OfferElement = default;
    }

    void Start() {
        InAppPurchasing.InitializePurchasing();
        
        InAppPurchasing.PurchaseCompleted += PurchaseCompletedHandler;
        InAppPurchasing.PurchaseFailed += PurchaseFailedHandler;

        _softCurrency = _currencyManager.GetCurrency(SOFT_CURRENCY_ID);
        
        SetupOffers();
    }

    void OnDestroy() {
        InAppPurchasing.PurchaseCompleted -= PurchaseCompletedHandler;
        InAppPurchasing.PurchaseFailed -= PurchaseFailedHandler;
    }

    // Successful purchase handler
    void PurchaseCompletedHandler(IAPProduct product) {
        var unityProduct = InAppPurchasing.GetProduct(product.Name);
        if (unityProduct != null) {
            var amount = decimal.ToInt32 (unityProduct.metadata.localizedPrice * 100);
            Analytic.NewInapp(unityProduct.metadata.isoCurrencyCode, amount, product.Type.ToString(), product.Id, "shop");
        }

        switch (product.Name) {
            case EM_IAPConstants.Product_Coin_Pack_1:
                var offerInfo1 = _offerInfos.FirstOrDefault(x => x.Name.Equals(EM_IAPConstants.Product_Coin_Pack_1));
                _softCurrency.Earn(offerInfo1.CoinsAmount);
                Analytic.CurrencyEarn(offerInfo1.CoinsAmount, "pack-bought", EM_IAPConstants.Product_Coin_Pack_1);
                break;
            case EM_IAPConstants.Product_Coin_Pack_2:
                var offerInfo2 = _offerInfos.FirstOrDefault(x => x.Name.Equals(EM_IAPConstants.Product_Coin_Pack_2));
                _softCurrency.Earn(offerInfo2.CoinsAmount);
                Analytic.CurrencyEarn(offerInfo2.CoinsAmount, "pack-bought", EM_IAPConstants.Product_Coin_Pack_2);
                break;
            case EM_IAPConstants.Product_Coin_Pack_3:
                var offerInfo3 = _offerInfos.FirstOrDefault(x => x.Name.Equals(EM_IAPConstants.Product_Coin_Pack_3));
                _softCurrency.Earn(offerInfo3.CoinsAmount);
                Analytic.CurrencyEarn(offerInfo3.CoinsAmount, "pack-bought", EM_IAPConstants.Product_Coin_Pack_3);
                break;
            case EM_IAPConstants.Product_Coin_Pack_4:
                var offerInfo4 = _offerInfos.FirstOrDefault(x => x.Name.Equals(EM_IAPConstants.Product_Coin_Pack_4));
                Analytic.CurrencyEarn(offerInfo4.CoinsAmount, "pack-bought", EM_IAPConstants.Product_Coin_Pack_4);
                _softCurrency.Earn(offerInfo4.CoinsAmount);
                break;
            case EM_IAPConstants.Product_Coin_Pack_5:
                var offerInfo5 = _offerInfos.FirstOrDefault(x => x.Name.Equals(EM_IAPConstants.Product_Coin_Pack_5));
                _softCurrency.Earn(offerInfo5.CoinsAmount);
                Analytic.CurrencyEarn(offerInfo5.CoinsAmount, "pack-bought", EM_IAPConstants.Product_Coin_Pack_5);
                break;
        }
    }

    // Failed purchase handler
    void PurchaseFailedHandler(IAPProduct product) {
        Debug.LogError("The purchase of product " + product.Name + " has failed.");
    }

    void SetupOffers() {
        foreach (OfferInfo offerInfo in _offerInfos) {
            var iapProduct = EM_Settings.InAppPurchasing.Products.FirstOrDefault(x => x.Name.Equals(offerInfo.Name));
            ProductMetadata data = InAppPurchasing.GetProductLocalizedData(iapProduct.Name);

            var cost = data != null ? data.localizedPriceString : iapProduct.Price;
            
            offerInfo.OfferElement.Setup(offerInfo.CoinsAmount + " <sprite=0>", cost);
        }
    }

    public void SendAnalytics() {
        Analytic.Send("store_opened");
    }

    public void BuyCoinPack1() {
        InAppPurchasing.Purchase(EM_IAPConstants.Product_Coin_Pack_1);
    }

    public void BuyCoinPack2() {
        InAppPurchasing.Purchase(EM_IAPConstants.Product_Coin_Pack_2);
    }

    public void BuyCoinPack3() {
        InAppPurchasing.Purchase(EM_IAPConstants.Product_Coin_Pack_3);
    }

    public void BuyCoinPack4() {
        InAppPurchasing.Purchase(EM_IAPConstants.Product_Coin_Pack_4);
    }

    public void BuyCoinPack5() {
        InAppPurchasing.Purchase(EM_IAPConstants.Product_Coin_Pack_5);
    }
}