using System.Linq;

using EasyMobile;

using UnityEngine;
using UnityEngine.Purchasing;

public class UICoinShopView : MonoBehaviour {
    [SerializeField] Transform _offerContainer = default;
    
    IAPProduct[] _products;

    void Start() {
        _products = EM_Settings.InAppPurchasing.Products;
    }

    void SetupOffers() {
        var offers = _offerContainer.GetComponentsInChildren<UIOfferElement>();

        foreach (UIOfferElement offer in offers) {
            var iapProduct = EM_Settings.InAppPurchasing.Products.FirstOrDefault(x => x.Name == offer.Name);
            ProductMetadata data = InAppPurchasing.GetProductLocalizedData(iapProduct.Name);
            offer.Setup(data.localizedTitle, data.localizedDescription, data.localizedPriceString, iapProduct);
        }
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