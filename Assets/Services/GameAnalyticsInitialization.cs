using GameAnalyticsSDK;

using UnityEngine;

public class GameAnalyticsInitialization : MonoBehaviour {
    void Awake() {
        GameAnalytics.Initialize();
    }
}
