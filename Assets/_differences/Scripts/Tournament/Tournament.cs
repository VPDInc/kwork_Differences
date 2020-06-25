﻿using System;
using System.Collections.Generic;
using System.Linq;

using Airion.Extensions;

using PlayFab;
using PlayFab.ClientModels;

using UnityEngine;

using Zenject;

public class Tournament : MonoBehaviour {
    public event Action<LeaderboardPlayer[]> Filled;
    public event Action<LeaderboardPlayer[]> FilledLastWinners;
    public event Action<LeaderboardPlayer[]> Completed;

    [SerializeField] bool _isDebugEnabled = true;

    [Inject] PlayFabLogin _login = default;

    readonly List<LeaderboardPlayer> _currentPlayers = new List<LeaderboardPlayer>();
    readonly List<LeaderboardPlayer> _prevPlayers = new List<LeaderboardPlayer>();

    const string LEADERBOARD_NAME = "Tournament";
    const string CURRENT_SAVED_COHORT_PREFS = "current_saved_cohort";
    const string PREV_SAVED_COHORT_PREFS = "prev_saved_cohort";
    const string CURRENT_LEADERBOARD_VERSION_PREFS = "current_leaderboard_version";
    const string PREV_LEADERBOARD_VERSION_PREFS = "prev_leaderboard_version";

    int _currentLeaderboardVersion = -1;
    int _prevLeaderboardVersion = -1;
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
        FilledLastWinners += (players) => {
            Log("Last ================");
            foreach (var player in players) {
                Log(player);
            }
            Log("/Last ================");
        };

        if (_login.IsLogged) {
            Load();
            return;
        }

        _login.PlayFabLogged += Load;
    }

    void Load() {
        Clear();
        // load current leaderboard
        LoadCurrentLeaderboard();
    }
    

    void Clear() {
        _currentPlayers.Clear();
        _prevPlayers.Clear();
    }

    void LoadCurrentLeaderboard() {
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest() {
            StatisticName = LEADERBOARD_NAME,
            MaxResultsCount = 0
        }, (result) => {
            OnCurrentLeaderboardLoaded(result.Version);
            
            
            // if (_lastVersion != result.Version) {
            //     if (savedIds.Length > 0) {
            //         RequestLastWinners(_lastVersion, savedIds);
            //     }
            //
            //     _lastVersion = result.Version;
            //     PlayerPrefs.SetInt(LAST_LEADERBOARD_VERSION_PREFS, _lastVersion);
            //     PlayerPrefs.DeleteKey(SAVED_COHORT_PREFS);
            //     savedIds = new string[0];
            // }
            //
            // if (savedIds.Length == 0) {
            //     Log("Try to generate leaderboard cohort");
            //     GenerateNewCohort();
            //     return;
            // }
            //
            // Log("Try to load saved leaderboard cohort");
            // LoadPlayers(savedIds);
            //
        }, err => {
            Err(err.GenerateErrorReport());
        });
    }

    void OnCurrentLeaderboardLoaded(int version) {
        Log($"Current active leaderboard version: {version}");
        
        _currentLeaderboardVersion = PlayerPrefs.GetInt(CURRENT_LEADERBOARD_VERSION_PREFS, -1);
        _prevLeaderboardVersion = PlayerPrefs.GetInt(PREV_LEADERBOARD_VERSION_PREFS, -1);
        var savedIds = PrefsExtensions.GetStringArray(CURRENT_SAVED_COHORT_PREFS);
        var prevIds = PrefsExtensions.GetStringArray(PREV_SAVED_COHORT_PREFS);

        //     if (current != _current saved) {
        //         if (current cohord)
        //           load profiles, load friends 
        //           Completed
        //         prev cohort = current cohort
        //Generate new cohort for current
        //     else 
        // load current prfiles
        // if last cohort
        // load last profiles
        
        if (_currentLeaderboardVersion != version) {
            if (savedIds.Length > 0) {
                LoadProfiles(savedIds, _currentLeaderboardVersion, (res) => {
                    SaveCohort(res.ToList(), PREV_SAVED_COHORT_PREFS);
                }, res => {
                    _prevPlayers.AddRange(res);
                    FilledLastWinners?.Invoke(res);
                    Completed?.Invoke(res);
                });
            }

            PlayerPrefs.SetInt(PREV_LEADERBOARD_VERSION_PREFS, _currentLeaderboardVersion);
            _prevLeaderboardVersion = _currentLeaderboardVersion;
            _currentLeaderboardVersion = version;
            PlayerPrefs.SetInt(CURRENT_LEADERBOARD_VERSION_PREFS, _currentLeaderboardVersion);
            GenerateNewCohort();
            return;
        }

        if (savedIds.Length == 0) {
            GenerateNewCohort();
        } else {
            LoadProfiles(savedIds, _currentLeaderboardVersion, (res)=> {},(res) => {
                Filled?.Invoke(res.ToArray());
            });
        }

        if (prevIds.Length > 0) {
            LoadProfiles(prevIds, _prevLeaderboardVersion, (res)=>{},(res) => {
                FilledLastWinners?.Invoke(res.ToArray());
            });
        }
    }

    void GenerateNewCohort() {
        AddScoreWithCallback(0, () => {
            PlayFabClientAPI.GetLeaderboardAroundPlayer(new GetLeaderboardAroundPlayerRequest() { StatisticName = LEADERBOARD_NAME, MaxResultsCount = 50 },
            result => {
                foreach (var player in result.Leaderboard) {
                    _currentPlayers.Add(new LeaderboardPlayer() {
                        DisplayName = player.DisplayName,
                        AvatarPath = player.Profile.AvatarUrl,
                        Id = player.PlayFabId,
                        Score = player.StatValue
                    });
                }
                SaveCohort(_currentPlayers, CURRENT_SAVED_COHORT_PREFS);
                Filled?.Invoke(_currentPlayers.ToArray());
                LoadCurrentFriends();
            }, 
            err=> Err(err.GenerateErrorReport()));
        });
    }
    
    void LoadCurrentFriends() {
        PlayFabClientAPI.GetFriendLeaderboard(new GetFriendLeaderboardRequest() {StatisticName = LEADERBOARD_NAME},
            result => {
                foreach (var player in result.Leaderboard) {
                    var loaded = false;
                    
                    for (int i = 0; i < _currentPlayers.Count; i++) {
                        var p = _currentPlayers[i];
                        if (p.Id.Equals(player.PlayFabId)) {
                            p.IsFriend = true;
                            _currentPlayers[i] = p;
                            loaded = true;
                            break;
                        }
                    }
                    
                    if (loaded)
                        continue;
                    
                    _currentPlayers.Add(new LeaderboardPlayer() {
                        DisplayName = player.DisplayName,
                        AvatarPath = player.Profile.AvatarUrl,
                        Id = player.PlayFabId,
                        Score = player.StatValue,
                        IsFriend = true
                    });
                }
                
                Log("Friends load completed");
                Filled?.Invoke(_currentPlayers.ToArray());
            }, err => {Err(err.GenerateErrorReport());});
    }
    
    void SaveCohort(List<LeaderboardPlayer> players, string prefs) {
        var ids = players.Select(player => player.Id).ToArray();
        PrefsExtensions.SetStringArray(prefs, ids);
    }
    
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

    void AddScoreWithCallback(int score, Action callback) {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate {
                    StatisticName = LEADERBOARD_NAME,
                    Value = score
                }
            }
        }, result => {
            callback?.Invoke();
            Log($"Score updated to {score}"); 
        }, err => Err(err.GenerateErrorReport()));
    }
    
    void LoadProfiles(string[] ids, int version, Action<LeaderboardPlayer[]> callbackWithoutFriends, Action<LeaderboardPlayer[]> callbackWithFriends) {
        var toLoad = ids.Length;
        var loaded = 0;
        List<LeaderboardPlayer> players = new List<LeaderboardPlayer>();
        foreach (var id in ids) {
            PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest() {
                PlayFabId = id,
                ProfileConstraints = new PlayerProfileViewConstraints() {
                    ShowStatistics = true,
                    ShowDisplayName = true
                }
            }, result => {
                var score = result.PlayerProfile.Statistics.Where(model => model.Name.Equals(LEADERBOARD_NAME) && model.Version == version).Select(model => model.Value).FirstOrDefault();
                players.Add(new LeaderboardPlayer() {
                    DisplayName = result.PlayerProfile.DisplayName,
                    AvatarPath = result.PlayerProfile.AvatarUrl,
                    Id = result.PlayerProfile.PlayerId,
                    Score = score
                });
                loaded++;
                if (loaded >= toLoad) {
                    callbackWithoutFriends?.Invoke(players.ToArray());
                    LoadFriends(version, players, res => { callbackWithFriends?.Invoke(res);});
                }
            }, fail => {
                loaded++;
                if (loaded >= toLoad) {
                    callbackWithoutFriends?.Invoke(players.ToArray());
                    LoadFriends(version, players, res => { callbackWithFriends?.Invoke(res);});
                }
                Err(fail.GenerateErrorReport());
            });
        }
    }
    
    void LoadFriends(int version, List<LeaderboardPlayer> players, Action<LeaderboardPlayer[]> callback) {
        PlayFabClientAPI.GetFriendLeaderboard(new GetFriendLeaderboardRequest() {
                StatisticName = LEADERBOARD_NAME,
                Version = version
            },
            result => {
                foreach (var player in result.Leaderboard) {
                    var loaded = false;
                    
                    for (int i = 0; i < players.Count; i++) {
                        var p = players[i];
                        if (p.Id.Equals(player.PlayFabId)) {
                            p.IsFriend = true;
                            players[i] = p;
                            loaded = true;
                            break;
                        }
                    }
                    
                    if (loaded)
                        continue;
                    
                    players.Add(new LeaderboardPlayer() {
                        DisplayName = player.DisplayName,
                        AvatarPath = player.Profile.AvatarUrl,
                        Id = player.PlayFabId,
                        Score = player.StatValue,
                        IsFriend = true
                    });
                }
                
                Log("Friends load completed");
                callback?.Invoke(players.ToArray());
            }, err => {
                callback?.Invoke(players.ToArray());
                Err(err.GenerateErrorReport());
            });
    }

    void Err(object message) {
        Debug.LogError($"[{GetType()}] {message}");
    }

    void Log(object message) {
        if (_isDebugEnabled)
            Debug.Log($"[{GetType()}] {message}");
    }
}



    //
    //
    // void LoadLeaderboard() {
    //     _players.Clear();
    //     _retrievedUsers = 0;
    //     _needToRetrieve = 0;
    //
    //     LoadLeaderboardInfo();
    // }
    //
    // void LoadLeaderboardInfo() {
    //     PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest() {
    //         StatisticName = LEADERBOARD_NAME,
    //         MaxResultsCount = 0
    //     }, (result) => {
    //         _lastVersion = PlayerPrefs.GetInt(LAST_LEADERBOARD_VERSION_PREFS, -1);
    //         var savedIds = PrefsExtensions.GetStringArray(SAVED_COHORT_PREFS);
    //         
    //         if (_lastVersion != result.Version) {
    //             if (savedIds.Length > 0) {
    //                 RequestLastWinners(_lastVersion, savedIds);
    //             }
    //
    //             _lastVersion = result.Version;
    //             PlayerPrefs.SetInt(LAST_LEADERBOARD_VERSION_PREFS, _lastVersion);
    //             PlayerPrefs.DeleteKey(SAVED_COHORT_PREFS);
    //             savedIds = new string[0];
    //         }
    //         
    //         if (savedIds.Length == 0) {
    //             Log("Try to generate leaderboard cohort");
    //             GenerateNewCohort();
    //             return;
    //         }
    //         
    //         Log("Try to load saved leaderboard cohort");
    //         LoadPlayers(savedIds);
    //         
    //     }, err => {
    //         Err(err.GenerateErrorReport());
    //     });
    // }
    //
    // void RequestLastWinners(int version, string[] ids) {
    //     PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest() {
    //         StatisticName = LEADERBOARD_NAME,
    //         Version = version
    //     }, (result) => {
    //         List<LeaderboardPlayer> playersInCohort = new List<LeaderboardPlayer>();
    //         foreach (var entry in result.Leaderboard) {
    //             if (ids.Contains(entry.PlayFabId)) {
    //                 playersInCohort.Add(new LeaderboardPlayer() {
    //                     AvatarPath = entry.Profile.AvatarUrl,
    //                     DisplayName = entry.DisplayName,
    //                     Id = entry.PlayFabId,
    //                     Score = entry.StatValue
    //                 });
    //             }
    //         }
    //         
    //         var ordered = playersInCohort.OrderByDescending(p => p.Score).ToArray();
    //         var winners = new List<LeaderboardPlayer>();
    //         for (int i = 0; i < 3; i++) {
    //             if (ordered.Length > 0 && ordered.Length > i) {
    //                 winners.Add(ordered[i]);
    //             }
    //         }
    //
    //         Completed?.Invoke(winners.ToArray());
    //     }, err => {
    //         Err(err.GenerateErrorReport());
    //     });
    // }
    //
    // void LoadPlayers(string[] savedIds) {
    //     _retrievedUsers = 0;
    //     _needToRetrieve = savedIds.Length;
    //     foreach (var id in savedIds) {
    //         PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest() {
    //             PlayFabId = id,
    //             ProfileConstraints = new PlayerProfileViewConstraints() {
    //                 ShowStatistics = true,
    //                 ShowDisplayName = true
    //             }
    //         }, OnUserIdRetrieveCompleted, OnUserIdFailedRetrieve);
    //     }
    // }
    //
    // void OnUserIdRetrieveCompleted(GetPlayerProfileResult result) {
    //     _retrievedUsers++;
    //     _players.Add(new LeaderboardPlayer() {
    //         DisplayName = result.PlayerProfile.DisplayName,
    //         AvatarPath = result.PlayerProfile.AvatarUrl,
    //         Id = result.PlayerProfile.PlayerId,
    //         Score = result.PlayerProfile.Statistics.Where(model => model.Name.Equals(LEADERBOARD_NAME)).Select(model => model.Value).LastOrDefault()
    //     });
    //     if (_retrievedUsers >= _needToRetrieve) {
    //         Filled?.Invoke(_players.ToArray());
    //     }
    // }
    //
    // void OnUserIdFailedRetrieve(PlayFabError err) {
    //     Err(err.GenerateErrorReport());
    //     _retrievedUsers++;
    //     if (_retrievedUsers >= _needToRetrieve) {
    //         Filled?.Invoke(_players.ToArray());
    //     }
    // }
    //
    // void GenerateNewCohort() {
    //     PlayFabClientAPI.GetLeaderboardAroundPlayer(new GetLeaderboardAroundPlayerRequest() {StatisticName = LEADERBOARD_NAME, MaxResultsCount = 50}, OnLeaderboardLoadCompleted, OnLeaderboardLoadFailed);
    //     AddScore(0);
    // }
    //
    // void OnLeaderboardLoadCompleted(GetLeaderboardAroundPlayerResult result) {
    //     foreach (var player in result.Leaderboard) {
    //         _players.Add(new LeaderboardPlayer() {
    //             DisplayName = player.DisplayName,
    //             AvatarPath = player.Profile.AvatarUrl,
    //             Id = player.PlayFabId,
    //             Score = player.StatValue
    //         });
    //     }
    //     SaveCohort();
    //     LoadFriends();
    // }
    //
    // void SaveCohort() {
    //     var ids = _players.Select(player => player.Id).ToArray();
    //     PrefsExtensions.SetStringArray(SAVED_COHORT_PREFS, ids);
    // }
    //
    // void LoadFriends() {
    //     PlayFabClientAPI.GetFriendLeaderboard(new GetFriendLeaderboardRequest() {StatisticName = LEADERBOARD_NAME}, OnFriendsLoadCompleted, OnFriendsLoadFailed);
    // }
    //
    // void OnFriendsLoadCompleted(GetLeaderboardResult result) {
    //     var ids = _players.Select(p => p.Id);
    //     foreach (var player in result.Leaderboard) {
    //         if (ids.Contains(player.PlayFabId))
    //             continue;
    //         
    //         _players.Add(new LeaderboardPlayer() {
    //             DisplayName = player.DisplayName,
    //             AvatarPath = player.Profile.AvatarUrl,
    //             Id = player.PlayFabId,
    //             Score = player.StatValue,
    //             IsFriend = true
    //         });
    //     }
    //     
    //     SaveCohort();
    //     Filled?.Invoke(_players.ToArray());
    //     Log("Friends load completed");
    // }
    //
    // void OnFriendsLoadFailed(PlayFabError err) {
    //     Err(err.GenerateErrorReport());
    //     Filled?.Invoke(_players.ToArray());
    //     Log("Friends load failed");
    // }
    //
    // void OnLeaderboardLoadFailed(PlayFabError err) {
    //     Err(err.GenerateErrorReport());
    // }
