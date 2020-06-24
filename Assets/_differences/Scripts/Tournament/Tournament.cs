using PlayFab;
using PlayFab.ClientModels;

using UnityEngine;

using Zenject;

public class Tournament : MonoBehaviour {
    [Inject] PlayFabLogin _login = default;
    
    const string LEADERBOARD_NAME = "Tournament";
    
    void Start() {
        if (_login.IsLogged) {
            GetLeaderboard();
            return;
        }

        _login.PlayFabLogged += GetLeaderboard;
    }

    void GetLeaderboard() {
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest() {StatisticName = LEADERBOARD_NAME}, 
            OnLeaderboardLoadCompleted, OnLeaderboardLoadFailed);
    }

    void OnLeaderboardLoadCompleted(GetLeaderboardResult result) {
        var entries = result.Leaderboard;
        Log(result.NextReset);
        Log(entries.Count);
    }
    
    void OnLeaderboardLoadFailed(PlayFabError error) {
        Err(error.GenerateErrorReport());
    }

    void Err(object message) {
        Debug.LogError($"[{GetType()}] {message}");
    } 
    
    void Log(object message) {
        Debug.Log($"[{GetType()}] {message}");
    } 
}
