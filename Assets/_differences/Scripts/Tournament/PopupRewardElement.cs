using System;
using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class PopupRewardElement : MonoBehaviour {
    [SerializeField] Image _icon = default;
    [SerializeField] TMP_Text _countText = default;
    [SerializeField] RewardInfo[] _rewardInfos = default;

    [Serializable]
    class RewardInfo {
        public RewardEnum RewardType = default;
        public Sprite Icon = default;
    }

    public void Setup(RewardEnum rewardType, int count) {
        var info = _rewardInfos.FirstOrDefault(x => x.RewardType == rewardType);
        _icon.sprite = info.Icon;
        _countText.text = "x " + count;
    }
}