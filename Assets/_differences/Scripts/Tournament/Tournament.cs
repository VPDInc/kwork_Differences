using System;
using System.Collections.Generic;
using System.Linq;

using Airion.Extensions;

using PlayFab;
using PlayFab.ClientModels;

using UnityEngine;

using Zenject;

public class Tournament : MonoBehaviour {
    public event Action<LeaderboardPlayer[]> Filled;
    
    [SerializeField] bool _isDebugEnabled = true;
    
    [Inject] PlayFabLogin _login = default;
    
    readonly List<LeaderboardPlayer> _players = new List<LeaderboardPlayer>();
    
    const string LEADERBOARD_NAME = "Tournament";
    const string SAVED_COHORT_PREFS = "saved_cohort";
    
    int _retrievedUsers = 0;
    int _needToRetrieve = 0;
    
    void Start() {
        Filled += (players) => {
            foreach (var player in players) {
                Log(player);
            }
        };
        if (_login.IsLogged) {
            LoadLeaderboard();
            return;
        }

        _login.PlayFabLogged += LoadLeaderboard;
    }
    
    // TODO: handle reset

    [ContextMenu(nameof(DebugAdd10Score))]
    void DebugAdd10Score() {
        AddScore(10);
    }

    void AddScore(int score) {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate {
                    StatisticName = LEADERBOARD_NAME,
                    Value = score
                }
            }
        }, result => {
            Log($"Score updated to {score}"); 
            LoadLeaderboard();
        }, err => Err(err.GenerateErrorReport()));
    }

    public struct LeaderboardPlayer {
        public string DisplayName;
        public string Id;
        public int Score;
        public string AvatarPath;

        public override string ToString() {
            return $"{Id}: {DisplayName}, {AvatarPath}. Score: {Score}";
        }
    }
    
    void LoadLeaderboard() {
        _players.Clear();
        _retrievedUsers = 0;
        _needToRetrieve = 0;
        
        var savedIds = PrefsExtensions.GetStringArray(SAVED_COHORT_PREFS);
        if (savedIds.Length == 0) {
            Log("Try to generate leaderboard cohort");
            GenerateNewCohort();
            return;
        }

        Log("Try to load saved leaderboard cohort");
        LoadPlayers(savedIds);
    }

    void LoadPlayers(string[] savedIds) {
        _retrievedUsers = 0;
        _needToRetrieve = savedIds.Length;
        foreach (var id in savedIds) {
            PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest() {
                PlayFabId = id,
                ProfileConstraints = new PlayerProfileViewConstraints() {
                    ShowStatistics = true,
                    ShowDisplayName = true
                }
            }, OnUserIdRetrieveCompleted, OnUserIdFailedRetrieve);
        }
    }

    void OnUserIdRetrieveCompleted(GetPlayerProfileResult result) {
        _retrievedUsers++;
        _players.Add(new LeaderboardPlayer() {
            DisplayName = result.PlayerProfile.DisplayName,
            AvatarPath = result.PlayerProfile.AvatarUrl,
            Id = result.PlayerProfile.PlayerId,
            Score = result.PlayerProfile.Statistics.Where(model => model.Name.Equals(LEADERBOARD_NAME)).Select(model => model.Value).FirstOrDefault()
        });
        if (_retrievedUsers >= _needToRetrieve) {
            Filled?.Invoke(_players.ToArray());
        }
    }
    
    void OnUserIdFailedRetrieve(PlayFabError err) {
        Err(err.GenerateErrorReport());
        _retrievedUsers++;
        if (_retrievedUsers >= _needToRetrieve) {
            Filled?.Invoke(_players.ToArray());
        }
    }

    void GenerateNewCohort() {
        PlayFabClientAPI.GetLeaderboardAroundPlayer(new GetLeaderboardAroundPlayerRequest() {StatisticName = LEADERBOARD_NAME, MaxResultsCount = 50}, OnLeaderboardLoadCompleted, OnLeaderboardLoadFailed);
        AddScore(0);
    }

    void OnLeaderboardLoadCompleted(GetLeaderboardAroundPlayerResult result) {
        foreach (var player in result.Leaderboard) {
            _players.Add(new LeaderboardPlayer() {
                DisplayName = player.DisplayName,
                AvatarPath = player.Profile.AvatarUrl,
                Id = player.PlayFabId,
                Score = player.StatValue
            });
        }

        SaveCohort();
        LoadFriends();
    }
    
    void SaveCohort() {
        var ids = _players.Select(player => player.Id).ToArray();
        PrefsExtensions.SetStringArray(SAVED_COHORT_PREFS, ids);
    }

    void LoadFriends() {
        PlayFabClientAPI.GetFriendLeaderboard(new GetFriendLeaderboardRequest() {StatisticName = LEADERBOARD_NAME}, OnFriendsLoadCompleted, OnFriendsLoadFailed);
    }

    void OnFriendsLoadCompleted(GetLeaderboardResult result) {
        var ids = _players.Select(p => p.Id);
        foreach (var player in result.Leaderboard) {
            if (ids.Contains(player.PlayFabId))
                continue;
            
            _players.Add(new LeaderboardPlayer() {
                DisplayName = player.DisplayName,
                AvatarPath = player.Profile.AvatarUrl,
                Id = player.PlayFabId,
                Score = player.StatValue
            });
        }
        
        SaveCohort();
        Filled?.Invoke(_players.ToArray());
        Log("Friends load completed");
    }
    
    void OnFriendsLoadFailed(PlayFabError err) {
        Err(err.GenerateErrorReport());
        Filled?.Invoke(_players.ToArray());
        Log("Friends load failed");
    }

    void OnLeaderboardLoadFailed(PlayFabError err) {
        Err(err.GenerateErrorReport());
    }


    void Err(object message) {
        Debug.LogError($"[{GetType()}] {message}");
    } 
    
    void Log(object message) {
        if (_isDebugEnabled)
            Debug.Log($"[{GetType()}] {message}");
    } 
}
