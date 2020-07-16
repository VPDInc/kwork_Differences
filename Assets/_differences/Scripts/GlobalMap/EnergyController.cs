using System;

using Airion.Currency;
using Airion.Extensions;

using UnityEngine;

using Zenject;

public class EnergyController : MonoBehaviour {
    [SerializeField] int _playCost = 10;
    [SerializeField] float _refillTime = 2.5f;
    [SerializeField] int _refillAmount = 1;
    
    [Inject] CurrencyManager _currencyManager = default;
    [Inject] GameplayController _gameplayController = default;

    Currency _energyCurrency = default;
    
    DateTime _nextRefillTimestamp = default;

    const string ENERGY_CURRENCY_ID = "Energy";
    const string LAST_ENERGY_REFILL_TIMESTAMP_ID = "energy_refill_timestamp";

    void Start() {
        _energyCurrency = _currencyManager.GetCurrency(ENERGY_CURRENCY_ID);
        _gameplayController.Completed += OnCompleted;
        HandlePassedTime();
    }

    void Update() {
        if (DateTime.UtcNow >= _nextRefillTimestamp) {
            RestoreEnergy();
        }
    }

    void OnCompleted(GameplayResult result) {
        if(result.IsCompleted)
            _energyCurrency.Earn(_playCost);
    }
    
    public bool IsCanPlay => _energyCurrency.IsEnough(_playCost);

    public bool TryPlay() {
        if (IsCanPlay) {
            SpendPlayCost();
        }

        return IsCanPlay;
    }

    public void SpendPlayCost() {
        _energyCurrency.Spend(_playCost);
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
        SaveTimestamp();
    }

    void SaveTimestamp() {
        _nextRefillTimestamp = DateTime.UtcNow + TimeSpan.FromMinutes(_refillTime);
        PrefsExtensions.SetDateTime(LAST_ENERGY_REFILL_TIMESTAMP_ID, _nextRefillTimestamp);
    }

    void LoadTimestamp() {
        _nextRefillTimestamp = PrefsExtensions.GetDateTime(LAST_ENERGY_REFILL_TIMESTAMP_ID, DateTime.Now);
    }
}