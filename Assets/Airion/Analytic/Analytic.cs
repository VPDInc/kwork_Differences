using System.Collections.Generic;
using System.Linq;

using UnityEngine.Analytics;

public static class Analytic {
    public static void Send(string eventName) {
        Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
    }
    
    public static void Send(string eventName, Dictionary<string, object> args) {
        var parameters = args.Select(pair => new Firebase.Analytics.Parameter(pair.Key, pair.Value.ToString())).ToArray();
        Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, parameters);
    }

    public static void Send(string eventName, params (string, object)[] args) {
        var dictionary = args.ToDictionary(item => item.Item1, item => item.Item2);
        Send(eventName, dictionary);
    }

    public static void SendBufferForce() {
    }
}
