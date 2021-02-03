using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Airion.Extensions;
using Doozy.Engine.UI;

namespace Differences.DailyRewards
{
    public class DailyRewardsSystem : MonoBehaviour
    {
        private const string LAST_TIME_PREFS = "daily_last_time";
        private const string CURRENT_DAY_PREFS = "current_day";
        private const string IS_OPENED_TODAY_PREFS = "is_opened_today";

        [SerializeField] private ItemCard[] _cards;
        [SerializeField] private UIView _window = default;

        [Header("Debug")]
        [SerializeField] private bool _isDebug = true;
        [SerializeField] private int _debugDay = 1;

        private int _currentDay = 1;
        private ItemCard _currentCard;
        private DateTime _lastOpenTimestamp;
        private bool _isOpenedToday = false;

        public void Initialize()
        {
            Load();

#if UNITY_EDITOR
            if (_isDebug)
            {
                _isOpenedToday = false;
                _currentDay = _debugDay;
                if (_currentDay > _cards.Length) _currentDay = 1;
            }
#endif

            if (_isOpenedToday) _window.Hide();
            else
            {
                foreach (var card in _cards)
                    card.Initialize(_currentDay);

                _window.Show();
                OpenCard();
            }
        }

        private void OpenCard()
        {
            _isOpenedToday = true;
            _lastOpenTimestamp = DateTime.Today;

            foreach (var card in _cards)
            {
                if (card.CheckStatus(StatusRewardCard.Waiting))
                {
                    _currentCard = card;
                    card.Open();
                    Save();
                    return;
                }
            }
        }

        public void GetReward()
        {
            if (_currentCard == null) return;

            _currentCard.GetReward(delegate
            {
                _window.Hide();
            });

            _currentCard = null;
        }

        #region Saving
        private void Save()
        {
            PrefsExtensions.SetDateTime(LAST_TIME_PREFS, _lastOpenTimestamp);
            PlayerPrefs.SetInt(CURRENT_DAY_PREFS, _currentDay);
            PrefsExtensions.SetBool(IS_OPENED_TODAY_PREFS, _isOpenedToday);
        }

        private void Load()
        {
            _lastOpenTimestamp = PrefsExtensions.GetDateTime(LAST_TIME_PREFS, DateTime.MinValue);
            _currentDay = PlayerPrefs.GetInt(CURRENT_DAY_PREFS, 1);
            _isOpenedToday = PrefsExtensions.GetBool(IS_OPENED_TODAY_PREFS, false);

            var daysPassedFromLastOpen = (DateTime.Today - _lastOpenTimestamp).Days;
            if (_isOpenedToday && daysPassedFromLastOpen >= 1)
            {
                IncreaseCurrentDay();
                Save();
            }
        }

        private void IncreaseCurrentDay()
        {
            _isOpenedToday = false;
            _currentDay += 1;

            if (_currentDay > _cards.Length)
                _currentDay = 1;
        }
        #endregion

        #region Debug
        [ContextMenu(nameof(DebugClear))]
        void DebugClear()
        {
            _currentDay = 1;
            _lastOpenTimestamp = DateTime.MinValue;
            _isOpenedToday = false;

            Save();
            Initialize();
        }
        #endregion

        [Serializable]
        private class ItemCard
        {
            [SerializeField] private RewardCard _card;
            [SerializeField] private RewardCardData _data;

            public bool CheckStatus(StatusRewardCard status)
            {
                if (_card.Status == status) return true;
                else return false;
            }

            public void Open() =>
                _card.Open();

            public void GetReward(Action onSuccses) =>
                _card.GetReward(onSuccses);

            public void Initialize(int day)
            {
                var numberOfDay = _data.NumberOfDay;

                if (day == numberOfDay) _card.Initialize(_data, StatusRewardCard.Waiting);
                else if (day < numberOfDay) _card.Initialize(_data, StatusRewardCard.Closed);
                else _card.Initialize(_data, StatusRewardCard.Opened);
            }
        }
    }
}
