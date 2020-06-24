using PlayFab;
using PlayFab.ClientModels;

using UnityEngine;

public class Tournament : MonoBehaviour {
    const string LEADERBOARD_NAME = "Tournament";
    
    void Start() {
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest() {StatisticName = LEADERBOARD_NAME}, 
            OnLeaderboardLoadCompleted, OnLeaderboardLoadFailed);
    }

    void OnLeaderboardLoadCompleted(GetLeaderboardResult result) {
        var entries = result.Leaderboard;
    }
    
    void OnLeaderboardLoadFailed(PlayFabError error) {
        Err(error.GenerateErrorReport());
    }

    void Err(string message) {
        Debug.LogError($"[{GetType()}] {message}");
    } 
}
