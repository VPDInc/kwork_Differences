﻿using System.Collections;

using Airion.Currency;
using Airion.Extensions;

using Doozy.Engine.UI;

using UnityEngine;

using Zenject;

namespace Airion.DailyRewards
{
    public class UIDailyReward : MonoBehaviour
    {
        [SerializeField] private UIView _window = default;
        [SerializeField] private UIDay _dayPrefab = default;
        [SerializeField] private Transform _content = default;
            
        [Inject] DailyRewardsSystem _dailyRewardsSystem = default;
        [Inject] CurrencyManager _currencyManager = default;
        [Inject] UIReceiveReward _receiveRewardWindow = default;

        void Start()
        {
            _dailyRewardsSystem.Initialized += OnInitialized;
            _dailyRewardsSystem.Filled += OnFilled;
            _dailyRewardsSystem.Rewarded += OnRewarded;
            _receiveRewardWindow.Received += OnRewardReceiveClick;
        }

        void OnDestroy()
        {
            _dailyRewardsSystem.Initialized -= OnInitialized;
            _dailyRewardsSystem.Filled -= OnFilled;
            _dailyRewardsSystem.Rewarded -= OnRewarded;
            _receiveRewardWindow.Received -= OnRewardReceiveClick;
        }

        void OnRewardReceiveClick()
        {
            _dailyRewardsSystem.Open();
            _receiveRewardWindow.Hide();
        }
        
        void OnInitialized(bool isNeedToOpenWindow)
        {
            if (isNeedToOpenWindow)
                StartCoroutine(OpeningRoutine());
        }

        IEnumerator OpeningRoutine()
        {
            Show();
            yield return new WaitForSeconds(1);

            var currentDayInfo = _dailyRewardsSystem.GetCurrentDayInfo();
            _receiveRewardWindow.Show(currentDayInfo.Reward);
        }

        public void OnHideClick() =>
            _window.Hide();

        public void Show() =>
            _window.Show();
        
        void OnFilled(DayInfo[] infos)
        {
            _content.DestroyAllChildren();

            foreach (var info in infos)
            {
                var infoObj = Instantiate(_dayPrefab, _content);
                infoObj.SetInfo(info);
            }
        }
        
        void OnRewarded(Reward reward)
        {
            // ДОРАБОТАТЬ
            var count = ((CurrencyReward)reward).Count;
            _currencyManager.GetCurrency("Soft").Earn(count);
            Analytic.CurrencyEarn(count, "daily-rewarded",
                _dailyRewardsSystem.GetCurrentDayInfo().DayNum.ToString());

            // УДАЛИТЬ КОММЕНТАРИЙ ПОСЛЕ УСПЕШНОГО ТЕСТА НОВОЙ СИСТЕМЫ
            //var amount = ((CurrencyReward) reward).Amount;
            //_currencyManager.GetCurrency("Soft").Earn(amount);
            //Analytic.CurrencyEarn(amount, "daily-rewarded", _dailyRewardsSystem.GetCurrentDayInfo().DayNum.ToString());
        }
    }
}
