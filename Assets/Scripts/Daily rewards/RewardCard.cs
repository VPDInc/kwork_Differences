using System.Collections;
using UnityEngine;
using Zenject;
using Airion.Currency;
using Differences;
using System;

[RequireComponent(typeof(Animator))]
public class RewardCard : MonoBehaviour
{
    private const string KEY_ANIMATION_OPEN = "Open";
    private const string KEY_ANIMATION_OPENED = "Opened";

    [SerializeField] private ItemAnimationRewared[] m_EffectsRewereds;

    [Header("Animation Config")]
    [SerializeField] [Min(0)] private float _keyAnimationOpenTime = 1.5f;
    [SerializeField] [Min(0)] private float _addRewardDelay = 1.3f;

    [Inject] private CurrencyManager _currencyManager = default;

    private Animator _animator;
    private RewardCardData _data;
    private int _countGottenRewards;

    public StatusRewardCard Status { get; private set; }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Initialize(RewardCardData data, StatusRewardCard status)
    {
        _data = data;
        Status = status;
        _countGottenRewards = 0;

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
    }

    private void AddReward(RewardEnum type, int count, Action onSuccses)
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

        _countGottenRewards++;
        if (_countGottenRewards == m_EffectsRewereds.Length) onSuccses?.Invoke();
    }

    public void GetReward(Action onSuccses)
    {
        foreach (var reward in _data.Rewards)
        {
            foreach (var effectRewered in m_EffectsRewereds)
            {
                if (effectRewered.Type == reward.Type)
                {
                    effectRewered.Play(delegate {
                        AddReward(reward.Type, reward.Count, onSuccses);
                    });
                    break;
                }
            }
        }
    }

    [Serializable]
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
