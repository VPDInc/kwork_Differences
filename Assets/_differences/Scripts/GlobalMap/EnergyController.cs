using System;

using Airion.Currency;
using Airion.Extensions;

using UnityEngine;

using Zenject;

public class EnergyController : MonoBehaviour {
    public int PlayCost => _playCost;
    public bool IsInfinityTimeOn => (DateTime.UtcNow - _infinityEnergyStartTimestamp).TotalHours <= _infinityEnergyDurationHours;
    
    [SerializeField] int _playCost = 10;
    [SerializeField] float _refillTime = 2.5f;
    [SerializeField] int _refillAmount = 1;
    [SerializeField] int _infinityEnergyDurationHours = 1;
    
    [Inject] CurrencyManager _currencyManager = default;
    [Inject] GameplayController _gameplayController = default;

    Currency _energyCurrency = default;

    DateTime _nextRefillTimestamp = default;
    DateTime _infinityEnergyStartTimestamp = default;

    const string ENERGY_CURRENCY_ID = "Energy";
    const string LAST_ENERGY_REFILL_TIMESTAMP_ID = "energy_refill_timestamp";
    const string INFINITY_ENERGY_START_TIMESTAMP_ID = "infinity_energy_start_timestamp";

    void Start() {
        _energyCurrency = _currencyManager.GetCurrency(ENERGY_CURRENCY_ID);
        _gameplayController.Completed += OnCompleted;
        HandlePassedTime();
        LoadInfinityTimestamp();
    }

    void Update() {
        if (DateTime.UtcNow >= _nextRefillTimestamp) {
            RestoreEnergy();
        }
    }

    void OnCompleted(GameplayResult result) {
        if (result.IsCompleted) {
            _energyCurrency.Earn(_playCost);
            Analytic.EnergyEarn(_playCost, "level-completed", LevelController.GetLastLevelNum().ToString());
        }
    }
    
    public bool IsCanPlay => _energyCurrency.IsEnough(_playCost);

    public bool TryPlay() {
        if (IsCanPlay) {
            SpendPlayCost();
        }

        return IsCanPlay;
    }

    public void SpendPlayCost() {
        if (!IsInfinityTimeOn) {
            _energyCurrency.Spend(_playCost);
            Analytic.EnergySpend(_playCost, "level-launched", LevelController.GetLastLevelNum().ToString());
        }
    }

    void HandlePassedTime() {
        LoadTimestamp();
        var delta = DateTime.UtcNow - _nextRefillTimestamp;
        var refillsPassed = Mathf.CeilToInt((float)delta.TotalMinutes / _refillTime);
        for (int i = 0; i < refillsPassed; i++) {
            RestoreEnergy();
        }
    }

    void RestoreEnergy() {
        _energyCurrency.Earn(_refillAmount);
        // Analytic.EnergyEarn(_playCost, "game-launched", "");
        SaveTimestamp();
    }

    void SaveTimestamp() {
        _nextRefillTimestamp = DateTime.UtcNow + TimeSpan.FromMinutes(_refillTime);
        PrefsExtensions.SetDateTime(LAST_ENERGY_REFILL_TIMESTAMP_ID, _nextRefillTimestamp);
    }

    void LoadTimestamp() {
        _nextRefillTimestamp = PrefsExtensions.GetDateTime(LAST_ENERGY_REFILL_TIMESTAMP_ID, DateTime.UtcNow);
    }

    public void AddInfinityTime() {
        _infinityEnergyStartTimestamp = DateTime.UtcNow;
        SaveTimestamp();
        
    }

    void SaveInfinityTimestamp() {
        PrefsExtensions.SetDateTime(INFINITY_ENERGY_START_TIMESTAMP_ID, _infinityEnergyStartTimestamp);
    }

    void LoadInfinityTimestamp() {
        _infinityEnergyStartTimestamp = PrefsExtensions.GetDateTime(INFINITY_ENERGY_START_TIMESTAMP_ID);
    }
}