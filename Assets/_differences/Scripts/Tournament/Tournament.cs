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
    public event Action<LeaderboardPlayer[]> Filled;
    public event Action<LeaderboardPlayer[]> FilledLastWinners;
    public event Action<LeaderboardPlayer[]> Completed;
    public DateTime NextReset { get; private set; }
    public LeaderboardPlayer[] CurrentPlayers => _currentPlayers.ToArray();

    [SerializeField] bool _isDebugEnabled = true;
    [SerializeField] float _reloadCooldown = 120;

    [Inject] CurrencyManager _currencyManager = default;
    [Inject] PlayFabInfo _info = default;
    [Inject] PlayFabLogin _login = default;
    
    Currency _rating = default;

    readonly List<LeaderboardPlayer> _currentPlayers = new List<LeaderboardPlayer>();
    readonly List<LeaderboardPlayer> _prevPlayers = new List<LeaderboardPlayer>();

    const string LEADERBOARD_NAME = "Tournament";
    const string CURRENT_SAVED_COHORT_PREFS = "current_saved_cohort";
    const string PREV_SAVED_COHORT_PREFS = "prev_saved_cohort";
    const string CURRENT_LEADERBOARD_VERSION_PREFS = "current_leaderboard_version";
    const string PREV_LEADERBOARD_VERSION_PREFS = "prev_leaderboard_version";
    const int COHORT_SIZE = 50;

    int _currentLeaderboardVersion = -1;
    int _prevLeaderboardVersion = -1;
    float _lastReload = 0;

    void Start() {
        _rating = _currencyManager.GetCurrency("Rating");
        
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

        if (_info.IsAccountInfoUpdated) {
            Load();
        }

        _info.AccountInfoRecieved += OnInfoReceived;
    }

    void OnInfoReceived(GetAccountInfoResult obj) {
        Load();
    }

    public void TryReloadTimed() {
        if (Time.time - _lastReload <= _reloadCooldown)
            return;

        _lastReload = Time.time;
        Load();
    }

    void Load() {
        Clear();
        LoadScore();
        LoadCurrentLeaderboard();
    }

    void LoadScore() {
        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest() {
            StatisticNames = new List<string>(){LEADERBOARD_NAME},
        }, result => {
            if (result.Statistics.Count == 0) {
                _rating.Set(0);
            } else {
                _rating.Set(result.Statistics[0].Value);
            }
        }, err => {
            Err(err.GenerateErrorReport());
        });
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
            OnCurrentLeaderboardLoaded(result.Version, result.NextReset.HasValue ? result.NextReset.Value : DateTime.MaxValue);
        }, err => {
            Err(err.GenerateErrorReport());
        });
    }

    void OnCurrentLeaderboardLoaded(int version, DateTime nextReset) {
        Log($"Current active leaderboard version: {version}");
        
        NextReset = nextReset;
        
        _currentLeaderboardVersion = PlayerPrefs.GetInt(CURRENT_LEADERBOARD_VERSION_PREFS, -1);
        _prevLeaderboardVersion = PlayerPrefs.GetInt(PREV_LEADERBOARD_VERSION_PREFS, -1);
        var savedIds = PrefsExtensions.GetStringArray(CURRENT_SAVED_COHORT_PREFS);
        var prevIds = PrefsExtensions.GetStringArray(PREV_SAVED_COHORT_PREFS);
        
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
            GenerateNewCohort(_currentLeaderboardVersion);
            return;
        }

        if (savedIds.Length <= COHORT_SIZE) {
            GenerateNewCohort(_currentLeaderboardVersion);
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

    void GenerateNewCohort(int version) {
        AddScoreWithCallback(0, () => {
            Log("Try to generate new cohort");
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
                    _currentPlayers.Add(Create(player.PlayFabId, player.DisplayName, player.StatValue, id));
                }
                SaveCohort(_currentPlayers, CURRENT_SAVED_COHORT_PREFS);
                Filled?.Invoke(_currentPlayers.ToArray());
                LoadCurrentFriends();
            }, 
            err=> Err(err.GenerateErrorReport()));
        });
    }

    LeaderboardPlayer Create(string id, string displayName, int score, string facebook, bool isFriend = false) {
        var player = new LeaderboardPlayer() {
            DisplayName = displayName,
            Id = id,
            Score = score,
            IsFriend = isFriend,
            Facebook = facebook,
            IsMe = _login.PlayerPlayfabId.Equals(id)
        };
        return player;
    }
    
    void LoadCurrentFriends() {
        PlayFabClientAPI.GetFriendLeaderboard(new GetFriendLeaderboardRequest() {
                StatisticName = LEADERBOARD_NAME,
                ProfileConstraints = new PlayerProfileViewConstraints() {
                    ShowLinkedAccounts = true,
                    ShowStatistics = true,
                    ShowDisplayName = true,
                },
                IncludeFacebookFriends = true
            },
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
                    
                    var accounts = player.Profile.LinkedAccounts;
                    var facebook = accounts.FirstOrDefault(acc => acc.Platform == LoginIdentityProvider.Facebook);
                    var id = facebook == null ? string.Empty : facebook.PlatformUserId;
                    _currentPlayers.Add(Create(player.PlayFabId, player.DisplayName, player.StatValue, id, true));
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
    
    public void AddScore(int score) {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate {
                    StatisticName = LEADERBOARD_NAME,
                    Value = score
                }
            }
        }, result => {
            Log($"Score updated to {score}"); 
            Load();
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
                Version = version,
                ProfileConstraints = new PlayerProfileViewConstraints() {
                    ShowLinkedAccounts = true,
                    ShowStatistics = true,
                    ShowDisplayName = true,
                },
                IncludeFacebookFriends = true
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

                    var accounts = player.Profile.LinkedAccounts;
                    var facebook = accounts.FirstOrDefault(acc => acc.Platform == LoginIdentityProvider.Facebook);
                    var fbId = facebook == null ? string.Empty : facebook.PlatformUserId;
                    players.Add(Create(player.PlayFabId, player.DisplayName, player.StatValue, fbId, true));
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