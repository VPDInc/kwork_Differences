using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Airion.DailyRewards;
using Zenject;
using Airion.Currency;
using Differences;
using System;

[RequireComponent(typeof(Animator))]
public class RewardCard : MonoBehaviour
{
    private const string KEY_ANIMATION_OPEN = "Open";
    private const string KEY_ANIMATION_OPENED = "Opened";
    public StatusRewardCard Status { get; private set; }

    [SerializeField] private ItemAnimationRewared[] m_EffectsRewereds;

    [Header("Animation Config")]
    [SerializeField] private float _keyAnimationOpenTime = 1.5f;
    [SerializeField] private float _addRewardDelay = 1.3f;

    [Inject] CurrencyManager _currencyManager = default;

    private Animator _animator;
    private RewardCardData _data;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Initialize(RewardCardData data, StatusRewardCard status)
    {
        _data = data;
        Status = status;

        switch (status)
        {
            case StatusRewardCard.Opened:
                _animator.SetTrigger(KEY_ANIMATION_OPENED);
                break;

            case StatusRewardCard.Closed:
            case StatusRewardCard.Waiting:
                Debug.Log($"Not have realization for {status} init card");
                break;
        }
    }

    public void Open()
    {
        if (Status != StatusRewardCard.Waiting) return;

        Status = StatusRewardCard.Opened;
        StartCoroutine(Opening());
    }

    private IEnumerator Opening()
    {
        _animator.SetTrigger(KEY_ANIMATION_OPEN);
        yield return new WaitForSeconds(_keyAnimationOpenTime);

        foreach (var reward in _data.Rewards)
        {
            yield return new WaitForSeconds(_addRewardDelay);
            PlayEffect(reward);
        }
    }

    private void AddReward(RewardEnum type, int count)
    {
        switch (type)
        {
            case RewardEnum.Soft:
                _currencyManager.GetCurrency(CurrencyConstants.SOFT).Earn(count);
                break;

            case RewardEnum.Aim:
                _currencyManager.GetCurrency(CurrencyConstants.AIM).Earn(count);
                break;
        }
    }

    private void PlayEffect(DailyRewardData data)
    {
        foreach (var effectRewered in m_EffectsRewereds)
        {
            if (effectRewered.Type == data.Type)
            {
                effectRewered.Play(delegate {
                    AddReward(data.Type, data.Count);
                });
                break;
            }
        }
    }

    [System.Serializable]
    private class ItemAnimationRewared
    {
        private const float PAUSED_ANINMATION_COINS = 0.1f;

        [SerializeField] private RewardEnum _type;

        [SerializeField] [Min(0)] private int _count;
        [SerializeField] private UITrailEffect _effect;
        [SerializeField] private RectTransform _effectStartPosition;
        [SerializeField] private RectTransform _effectFinishPosition;

        public RewardEnum Type => _type;

        //TODO 13.01.2021 REFACTORING!
        float countfx = 0;
        public void Play(Action onSuccses)
        {
            for (int i = 0; i < _count; i++)
            {
                var coinFx = Instantiate(_effect, _effectStartPosition);
                coinFx.Setup(_effectFinishPosition.position, PAUSED_ANINMATION_COINS * i, delegate {
                    ++countfx;

                    if (_count == countfx)
                    {
                        countfx = 0;
                        onSuccses?.Invoke();
                    };
                });
            }
        }
    }
}
