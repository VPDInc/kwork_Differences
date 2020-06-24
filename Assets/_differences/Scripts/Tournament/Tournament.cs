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
    public event Action<LeaderboardPlayer[]> Completed;

    
    [SerializeField] bool _isDebugEnabled = true;
    
    [Inject] PlayFabLogin _login = default;
    
    readonly List<LeaderboardPlayer> _players = new List<LeaderboardPlayer>();
    
    const string LEADERBOARD_NAME = "Tournament";
    const string SAVED_COHORT_PREFS = "saved_cohort";
    const string LAST_LEADERBOARD_VERSION_PREFS = "last_leaderboard_version";
    
    int _retrievedUsers = 0;
    int _needToRetrieve = 0;
    int _lastVersion = -1;
    DateTime _nextReset;
    
    void Start() {
        Filled += (players) => {
            foreach (var player in players) {
                Log(player);
            }
        };
        Completed += (players) => {
            Log("Winners ==============");
            foreach (var player in players) {
                Log(player);
            }
            Log("/Winners ==============");
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

        LoadLeaderboardInfo();
    }

    void LoadLeaderboardInfo() {
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest() {
            StatisticName = LEADERBOARD_NAME,
            MaxResultsCount = 0
        }, (result) => {
            _lastVersion = PlayerPrefs.GetInt(LAST_LEADERBOARD_VERSION_PREFS, -1);
            var savedIds = PrefsExtensions.GetStringArray(SAVED_COHORT_PREFS);
            
            if (_lastVersion != result.Version) {
                if (savedIds.Length > 0) {
                    RequestLastWinners(_lastVersion, savedIds);
                }

                _lastVersion = result.Version;
                PlayerPrefs.SetInt(LAST_LEADERBOARD_VERSION_PREFS, _lastVersion);
                PlayerPrefs.DeleteKey(SAVED_COHORT_PREFS);
                savedIds = new string[0];
            }
            
            if (savedIds.Length == 0) {
                Log("Try to generate leaderboard cohort");
                GenerateNewCohort();
                return;
            }
            
            Log("Try to load saved leaderboard cohort");
            LoadPlayers(savedIds);
            
        }, err => {
            Err(err.GenerateErrorReport());
        });
    }

    void RequestLastWinners(int version, string[] ids) {
        
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest() {
            StatisticName = LEADERBOARD_NAME,
            Version = version
        }, (result) => {
            List<LeaderboardPlayer> playersInCohort = new List<LeaderboardPlayer>();
            foreach (var entry in result.Leaderboard) {
                if (ids.Contains(entry.PlayFabId)) {
                    playersInCohort.Add(new LeaderboardPlayer() {
                        AvatarPath = entry.Profile.AvatarUrl,
                        DisplayName = entry.DisplayName,
                        Id = entry.PlayFabId,
                        Score = entry.StatValue
                    });
                }
            }
            
            var ordered = playersInCohort.OrderByDescending(p => p.Score).ToArray();
            var winners = new List<LeaderboardPlayer>();
            for (int i = 0; i < 3; i++) {
                if (ordered.Length > 0 && ordered.Length > i) {
                    winners.Add(ordered[i]);
                }
            }

            Completed?.Invoke(winners.ToArray());
        }, err => {
            Err(err.GenerateErrorReport());
        });
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
            Score = result.PlayerProfile.Statistics.Where(model => model.Name.Equals(LEADERBOARD_NAME)).Select(model => model.Value).LastOrDefault()
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
