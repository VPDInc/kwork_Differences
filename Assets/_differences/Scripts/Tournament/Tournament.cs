using System;
using System.Collections.Generic;
using System.Linq;

using Airion.Currency;
using Airion.Extensions;

using PlayFab;
using PlayFab.ClientModels;

using UnityEngine;

using Zenject;

using Currency = Airion.Currency.Currency;

public class Tournament : MonoBehaviour {
    public event Action<LeaderboardPlayer[]> CurrentFilled;
    public event Action<LeaderboardPlayer[]> LastWinnersFilled;
    // TODO: Make it empty
    public event Action<LeaderboardPlayer[]> Completed;
    
    public DateTime NextReset { get; private set; }

    [SerializeField] bool _isDebugEnabled = true;
    [SerializeField] float _reloadCooldown = 120;

    [Inject] CurrencyManager _currencyManager = default;
    [Inject] PlayFabInfo _info = default;
    [Inject] PlayFabLogin _login = default;
    
    Currency _rating = default;

    readonly List<LeaderboardPlayer> _currentPlayers = new List<LeaderboardPlayer>();
    readonly List<string> _friends = new List<string>();
    // readonly List<LeaderboardPlayer> _prevPlayers = new List<LeaderboardPlayer>();

    const string LEADERBOARD_NAME = "Tournament";
    const string LAST_GENERATED_COHORT_PREFS = "current_saved_cohort";
    const string PREV_GENERATED_COHORT_PREFS = "prev_saved_cohort";
    const string LAST_USED_LEADERBOARD_VERSION_PREFS = "current_leaderboard_version";
    const string PREV_USED_LEADERBOARD_VERSION_PREFS = "prev_leaderboard_version";
    
    const int COHORT_SIZE = 50;

    // int _currentLeaderboardVersion = -1;
    // int _prevLeaderboardVersion = -1;
    float _lastReloadTimestamp = 0;

    void Start() {
        _rating = _currencyManager.GetCurrency("Rating");
        
        CurrentFilled += (players) => {
            foreach (var player in players) {
                Log(player);
            }
        };
        
        // Completed += () => {
           // Log("Completed");
        // };
        
        LastWinnersFilled += (players) => {
            Log("Last ================");
            foreach (var player in players) {
                Log(player);
            }
            Log("/Last ================");
        };

        if (_info.IsAccountInfoUpdated) {
            Load();
        }

        _info.AccountInfoRecieved += OnInfoReceived;
    }
    
    public void AddScore(int score) {
        AddScoreWithCallback(score, Load);
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
            Log($"Score updated to {score}"); 
            callback?.Invoke();
        }, err => Err(err.GenerateErrorReport()));
    }

    public void TryReloadTimed() {
        if (Time.time - _lastReloadTimestamp <= _reloadCooldown)
            return;

        _lastReloadTimestamp = Time.time;
        Load();
    }
    
    void OnInfoReceived(GetAccountInfoResult obj) => Load();

    void Load() {
        Clear();
        LoadFriends(LoadCurrentLeaderboard);
    }

    void LoadFriends(Action action) {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest() {
            IncludeFacebookFriends = true,
        }, res => {
            _friends.AddRange(res.Friends.Select(f => f.FriendPlayFabId));
            action.Invoke();
        }, fail => {
            action.Invoke();
            Err(fail.GenerateErrorReport());   
        });
    }

    void Clear() {
        _currentPlayers.Clear();
        _friends.Clear();
    }

    void LoadCurrentLeaderboard() {
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest() {
            StatisticName = LEADERBOARD_NAME,
            MaxResultsCount = 0
        }, (result) => {
            var version = result.Version;
            Log($"Current active leaderboard version: {version}");
            LoadScore(version);
            var resetTimestamp = result.NextReset.HasValue ? result.NextReset.Value : DateTime.MaxValue;
            LoadCurrentCohort(version, resetTimestamp);
            LoadPrevCohort(version);
            CheckCompletion();
        }, err => {
            Err(err.GenerateErrorReport());
        });
    }

    void LoadCurrentCohort(int resultVersion, DateTime resetTimestamp) {
        NextReset = resetTimestamp;
        
        var lastUsedVersion = PlayerPrefs.GetInt(LAST_USED_LEADERBOARD_VERSION_PREFS, -1);
        var lastGeneratedCohort = PrefsExtensions.GetStringArray(LAST_GENERATED_COHORT_PREFS);

        if (lastUsedVersion == resultVersion) {
            if (lastGeneratedCohort.Length >= COHORT_SIZE) {
                LoadProfiles(lastGeneratedCohort, lastUsedVersion, res => {
                    _currentPlayers.AddRange(res);
                    CurrentFilled?.Invoke(_currentPlayers.ToArray());
                });
                
                return;
            }
            
        } else {
            if ((resultVersion - lastUsedVersion == 1) && lastUsedVersion != -1) {
                PlayerPrefs.SetInt(PREV_USED_LEADERBOARD_VERSION_PREFS, lastUsedVersion);
                PrefsExtensions.SetStringArray(PREV_GENERATED_COHORT_PREFS, lastGeneratedCohort);
                // TODO: Completed event raise = true
            }
        }
        
        GenerateNewCohort(resultVersion, res => {
            _currentPlayers.AddRange(res);
            PlayerPrefs.SetInt(LAST_USED_LEADERBOARD_VERSION_PREFS, resultVersion);
            SaveCohort(_currentPlayers, LAST_GENERATED_COHORT_PREFS);
            CurrentFilled?.Invoke(_currentPlayers.ToArray());
        });
    }
    
    void SaveCohort(List<LeaderboardPlayer> players, string prefs) {
        var ids = players.Select(player => player.Id).ToArray();
        PrefsExtensions.SetStringArray(prefs, ids);
    }
    
    void LoadProfiles(string[] ids, int version, Action<LeaderboardPlayer[]> callback) {
        var toLoad = ids.Length;
        var loaded = 0;
        List<LeaderboardPlayer> players = new List<LeaderboardPlayer>();
        foreach (var id in ids) {
            PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest() {
                PlayFabId = id,
                ProfileConstraints = new PlayerProfileViewConstraints() {
                    ShowStatistics = true,
                    ShowDisplayName = true,
                    ShowLinkedAccounts = true
                }
            }, result => {
                var score = result.PlayerProfile.Statistics.Where(model => model.Name.Equals(LEADERBOARD_NAME) && model.Version == version).Select(model => model.Value).FirstOrDefault();
                var accounts = result.PlayerProfile.LinkedAccounts;
                var facebook = accounts.FirstOrDefault(acc => acc.Platform == LoginIdentityProvider.Facebook);
                var fbId = facebook == null ? string.Empty : facebook.PlatformUserId;
                players.Add(Create(result.PlayerProfile.PlayerId, result.PlayerProfile.DisplayName, score, fbId));
                loaded++;
                if (loaded >= toLoad) {
                    callback?.Invoke(players.ToArray());
                }
            }, fail => {
                loaded++;
                if (loaded >= toLoad) {
                    callback?.Invoke(players.ToArray());
                }
                Err(fail.GenerateErrorReport());
            });
        }
    }
    
    LeaderboardPlayer Create(string id, string displayName, int score, string facebook) {
        var player = new LeaderboardPlayer() {
            DisplayName = displayName,
            Id = id,
            Score = score,
            IsFriend = _friends.Contains(id),
            Facebook = facebook,
            IsMe = _login.PlayerPlayfabId.Equals(id)
        };
        return player;
    }
    
    void GenerateNewCohort(int version, Action<LeaderboardPlayer[]> completedCallback) {
        AddScoreWithCallback(0, () => {
            Log("Try to generate new cohort");
            var players = new List<LeaderboardPlayer>();
            PlayFabClientAPI.GetLeaderboardAroundPlayer(new GetLeaderboardAroundPlayerRequest() {
                    StatisticName = LEADERBOARD_NAME, 
                    MaxResultsCount = 50, 
                    Version = version,
                    ProfileConstraints = new PlayerProfileViewConstraints() {
                        ShowLinkedAccounts = true,
                        ShowStatistics = true,
                        ShowDisplayName = true,
                    }
                },
                result => {
                    foreach (var player in result.Leaderboard) {
                        var accounts = player.Profile.LinkedAccounts;
                        var facebook = accounts.FirstOrDefault(acc => acc.Platform == LoginIdentityProvider.Facebook);
                        var id = facebook == null ? string.Empty : facebook.PlatformUserId;
                        players.Add(Create(player.PlayFabId, player.DisplayName, player.StatValue, id));
                    }
                    
                    LoadProfiles(_friends.ToArray(), version, friends => {
                        foreach (var friend in friends) {
                            if (!players.Any(p => p.Id.Equals(friend.Id))) {
                                players.Add(friend);
                            }
                        }
                        
                        completedCallback?.Invoke(players.ToArray());
                    });
                }, 
                err=> Err(err.GenerateErrorReport()));
        });
    }

    void LoadPrevCohort(int currentVersion) {
        // var prevUsedVersion = Prefs.Load
        // var prevCohort = Prefs.Load
        // if (currentVersion - prevUsedVersion == 1) {
        // if (prevCohort > 0) {
            // LOAD_PREV_COHORT
            // PREV_LOADED_EVENT
            // return;
        //}
        
        // var prevVersion = currentVersion - 1;
        // if (prevVersion >= 0) {
            // LOAD LEADERBOARD (Prev) with size 50
            // SAVE PREV COHORT
            // PREV LOADED EVENT
            // return;
        // }
        
        // PREV LOADED WITH EMPTY
    // }

    }
    
    void CheckCompletion() {
        // raise if true in load current cohort
        // TODO: CHECK completion by version
        // raise while not closed window by user
    }

    void LoadScore(int version) {
        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest() {
            StatisticNames = new List<string>(){LEADERBOARD_NAME},
        }, result => {
            if (result.Statistics.Count == 0) {
                _rating.Set(0);
            } else {
                var score = result.Statistics.Where(model => model.Version == version).Select(model => model.Value).FirstOrDefault();
                _rating.Set(score);
            }
        }, err => {
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