using System.Collections.Generic;
using System.Linq;

using Firebase.Analytics;

using GameAnalyticsSDK;

using UnityEngine.Analytics;

public static class Analytic {
    public static void Send(string eventName) {
        FirebaseAnalytics.LogEvent(eventName);
    }
    
    public static void Send(string eventName, Dictionary<string, object> args) {
        var parameters = args.Select(pair => new Firebase.Analytics.Parameter(pair.Key, pair.Value.ToString())).ToArray();
        FirebaseAnalytics.LogEvent(eventName, parameters);
    }

    public static void Send(string eventName, params (string, object)[] args) {
        var dictionary = args.ToDictionary(item => item.Item1, item => item.Item2);
        Send(eventName, dictionary);
    }

    public static void CurrencyEarn(int amount, string itemType, string item) {
        GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "soft", amount, itemType, item);
        var par = new[] {
            new Parameter("event", "earn"), 
            new Parameter("currency", "soft"), 
            new Parameter("amount", amount),
            new Parameter("item-type", itemType), 
            new Parameter("id", item), 
        };
        FirebaseAnalytics.LogEvent("resource-event", par);
    }
    
    public static void CurrencySpend(int amount, string itemType, string item) {
        GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "soft", amount, itemType, item);
        var par = new[] {
            new Parameter("event", "spend"), 
            new Parameter("currency", "soft"), 
            new Parameter("amount", amount),
            new Parameter("item-type", itemType), 
            new Parameter("id", item), 
        };
        FirebaseAnalytics.LogEvent("resource-event", par);
    }
    
    public static void EnergyEarn(int amount, string itemType, string item) {
        GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "energy", amount, itemType, item);
        var par = new[] {
            new Parameter("event", "earn"), 
            new Parameter("currency", "energy"), 
            new Parameter("amount", amount),
            new Parameter("item-type", itemType), 
            new Parameter("id", item), 
        };
        FirebaseAnalytics.LogEvent("resource-event", par);
    }
    
    public static void EnergySpend(int amount, string itemType, string item) {
        GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "energy", amount, itemType, item);
        var par = new[] {
            new Parameter("event", "spend"), 
            new Parameter("currency", "energy"), 
            new Parameter("amount", amount),
            new Parameter("item-type", itemType), 
            new Parameter("id", item), 
        };
        FirebaseAnalytics.LogEvent("resource-event", par);
    }

    public static void SendBufferForce() {
    }

    public static void NewInapp(string currency, int amount, string itemType, string id, string cart) {
        GameAnalytics.NewBusinessEvent(currency, amount, itemType, id, cart);
        var par = new[] {
            new Parameter("currency", currency), 
            new Parameter("amount", amount / 100.0f), 
            new Parameter("item-type", itemType), 
            new Parameter("id", id),
            new Parameter("cart", cart), 
        };
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, par);
    }
}
