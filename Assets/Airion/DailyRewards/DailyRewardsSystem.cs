using System;
using System.Collections;
using System.Collections.Generic;

using Airion.Extensions;

using UnityEngine;

namespace Airion.DailyRewards {
   public class DailyRewardsSystem : MonoBehaviour {
      /// <summary>
      /// Fired when system has been initialized.
      /// IsNeedToOpenWindow 
      /// </summary>
      public event Action<bool> Initialized;
      public event Action<DayInfo[]> Filled;
      public event Action<Reward> Rewarded;
      
      [SerializeField] int _daysCount = 5;
      [SerializeField] Reward[] _rewards = default;
      [SerializeField] bool _isDebugEnabled = true;

      DateTime Now => _isDebugEnabled ? _debugNowDateTime : DateTime.Today;
      int _currentDay = 0;
      DateTime _lastOpenTimestamp;
      DateTime _debugNowDateTime;
      bool _isOpenedToday = false;
      
      readonly List<DayInfo> _infos = new List<DayInfo>();

      const string LAST_TIME_PREFS = "daily_last_time";
      const string CURRENT_DAY_PREFS = "current_day";
      const string IS_OPENED_TODAY_PREFS = "is_opened_today";
      
      IEnumerator Start() {
         _debugNowDateTime = DateTime.Today;
         yield return new WaitForEndOfFrame();
         Initialize();
      }
      
      void OnValidate() {
         Debug.Assert(_rewards.Length > 0, $"[{GetType()}] No one daily reward assigned!");
      }
      
      [ContextMenu(nameof(Open))]
      public void Open() {
         if (_isOpenedToday)
            return;

         _isOpenedToday = true;
         _lastOpenTimestamp = Now;
         
         Rewarded?.Invoke(_infos[_currentDay].Reward);
         
         Save();
         Fill();
      }
      
      public DayInfo GetCurrentDayInfo() {
         return _infos[_currentDay];
      }
      
      void Save() {
         PrefsExtensions.SetDateTime(LAST_TIME_PREFS, _lastOpenTimestamp);
         PlayerPrefs.SetInt(CURRENT_DAY_PREFS, _currentDay);
         PrefsExtensions.SetBool(IS_OPENED_TODAY_PREFS, _isOpenedToday);
      }
      
      [ContextMenu(nameof(Initialize))]
      void Initialize() {
         Load();
         Fill();

         var isNeedToOpenWindow = !_isOpenedToday;
         Initialized?.Invoke(isNeedToOpenWindow);
      }
      
      void Load() {
         _lastOpenTimestamp = PrefsExtensions.GetDateTime(LAST_TIME_PREFS, DateTime.MinValue);
         _currentDay = PlayerPrefs.GetInt(CURRENT_DAY_PREFS, 0);
         _isOpenedToday = PrefsExtensions.GetBool(IS_OPENED_TODAY_PREFS, false);
         
         var daysPassedFromLastOpen = (Now - _lastOpenTimestamp).Days;
         if (_isOpenedToday && daysPassedFromLastOpen >= 1) {
            IncreaseCurrentDay();
            Save();
         }
      }

      void IncreaseCurrentDay() {
         _isOpenedToday = false;
         _currentDay += 1;
         if (_currentDay >= _daysCount)
            _currentDay = 0;
      }
      
      void Fill() {
         _infos.Clear();
         
         for (var i = 0; i < _daysCount; i++) {
            _infos.Add(GetDayInfo(i));
         }
         
         Filled?.Invoke(_infos.ToArray());
      }

      DayInfo GetDayInfo(int dayNum) {
         var info = new DayInfo();
         var rewardIndex = Mathf.RoundToInt(Mathf.Repeat(dayNum, _rewards.Length));
         var reward = _rewards[rewardIndex];
         
         info.Status = dayNum <= _currentDay ? DayStatus.Opened : DayStatus.NotOpened;
         
         if (DaysPassedSince(dayNum) == 0) {
            if (!_isOpenedToday)
               info.Status = DayStatus.NotOpened;
            
            info.Additional = AdditionalDaySetting.Today;
         }
         
         if (DaysPassedSince(dayNum) == 1) {
            info.Additional = AdditionalDaySetting.Tomorrow;
         }

         info.Reward = reward;
         info.DayNum = dayNum;
         
         if (_isDebugEnabled)
            Debug.Log(info);
         
         return info;
      }
      
      int DaysPassedSince(int dayNum) => dayNum - _currentDay;

      #region Debug

      [ContextMenu(nameof(DebugIncreaseAddDay))]
      void DebugIncreaseAddDay() {
         _debugNowDateTime = _debugNowDateTime.AddDays(1);
         Initialize();
      }

      [ContextMenu(nameof(DebugDecreaseDay))]
      void DebugDecreaseDay() {
         _debugNowDateTime = _debugNowDateTime.AddDays(-1);
         Initialize();
      }
      
      [ContextMenu(nameof(DebugClear))]
      void DebugClear() {
         _currentDay = 0;
         _lastOpenTimestamp = DateTime.MinValue;
         _isOpenedToday = false;
         Save();
         Initialize();
      }
      
      #endregion // Debug
   }
}
