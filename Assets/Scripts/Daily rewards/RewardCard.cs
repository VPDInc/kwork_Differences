using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Airion.DailyRewards;
using Zenject;
using Airion.Currency;

[RequireComponent(typeof(Animator))]
public class RewardCard : MonoBehaviour
{
    private const string KEY_ANIMATION_OPEN = "Open";
    private const string KEY_ANIMATION_OPENED = "Opened";

    [SerializeField] private ItemAnimationRewared[] m_EffectsRewereds;

    [Inject] CurrencyManager _currencyManager = default;

    private Animator _animator;

    private RewardCardData _data;
    private StatusRewardCard _status;

    public StatusRewardCard Status => _status;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Initialize(RewardCardData data, StatusRewardCard status)
    {
        _data = data;
        _status = status;

        switch (status)
        {
            case StatusRewardCard.Opened:
                _animator.SetTrigger(KEY_ANIMATION_OPENED);
                break;

            case StatusRewardCard.Closed:
                // Нет реализации
                break;

            case StatusRewardCard.Waiting:
                // Нет реализации
                break;
        }
    }

    public void Open()
    {
        if (_status != StatusRewardCard.Waiting) return;

        _status = StatusRewardCard.Opened;
        StartCoroutine(Opening());
    }

    private IEnumerator Opening()
    {
        _animator.SetTrigger(KEY_ANIMATION_OPEN);
        yield return new WaitForSeconds(1.5f);

        foreach (var reward in _data.Rewards)
            PlayEffect(reward.Type);

        foreach (var reward in _data.Rewards)
        {
            if (reward.Type == DailyRewardData.TypeReward.Coin)
            {
                yield return new WaitForSeconds(1.3f);
                AddReward(DailyRewardData.TypeReward.Coin, reward.Count);
            }
            else AddReward(DailyRewardData.TypeReward.Target, reward.Count);
        }
    }

    private void AddReward(DailyRewardData.TypeReward type, int count)
    {
        switch (type)
        {
            case DailyRewardData.TypeReward.Coin:
                _currencyManager.GetCurrency("Soft").Earn(count);
                break;

            case DailyRewardData.TypeReward.Target:
                _currencyManager.GetCurrency("Aim").Earn(count);
                break;
        }
    }

    private void PlayEffect(DailyRewardData.TypeReward type)
    {
        foreach (var effectRewered in m_EffectsRewereds)
        {
            if (effectRewered.Type == type)
            {
                effectRewered.Play();
                break;
            }
        }
    }

    [System.Serializable]
    private class ItemAnimationRewared
    {
        private const float PAUSED_ANINMATION_COINS = 0.1f;

        [SerializeField] private DailyRewardData.TypeReward _type;

        [SerializeField] [Min(0)] private int _count;
        [SerializeField] private UITrailEffect _effect;
        [SerializeField] private RectTransform _effectStartPosition;
        [SerializeField] private RectTransform _effectFinishPosition;

        public DailyRewardData.TypeReward Type => _type;

        public void Play()
        {
            for (int i = 0; i < _count; i++)
            {
                var coinFx = Instantiate(_effect, _effectStartPosition);
                coinFx.Setup(_effectFinishPosition.position, PAUSED_ANINMATION_COINS * i);
            }
        }
    }
}
