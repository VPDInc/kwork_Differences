using Airion.Extensions;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;

public class UIFinishLevelView : MonoBehaviour {
    [Header("References")] [SerializeField]
    TMP_Text _levelLabel = default;
    [SerializeField] TMP_Text _coinRewardLabel = default;
    [SerializeField] TMP_Text _playerNameLabel = default;
    [SerializeField] Image _profileIcon = default;
    [SerializeField] Transform _infoHolder = default;

    [Header("Prefabs")] [SerializeField] UIPictureResultInfo _pictureResultInfoPrefab = default;

    UIView _currentView = default;

    const string LEVEL_NAME_PREFIX = "Level ";

    void Awake() {
        _currentView = GetComponent<UIView>();
    }

    void Start() {
        SetPlayerName("Babaduk");
    }

    //DUMMY
    public void Show(int levelNum, int picturesCount, int differencesCount, int coinReward) {
        Show();
        SetLevelName(levelNum);
        SetCoinsAmount(coinReward);
        Setup(picturesCount, differencesCount);
    }

    void Show() {
        _currentView.Show();
    }

    public void Hide() {
        _currentView.Hide();
    }

    void SetLevelName(int levelNum) {
        _levelLabel.text = LEVEL_NAME_PREFIX + (levelNum + 1);
    }

    void SetCoinsAmount(int coinsAmount) {
        _coinRewardLabel.text = coinsAmount.ToString();
    }

    void SetPlayerName(string name) {
        _playerNameLabel.text = name;
    }

    public void SetPlayerProfileIcon(Sprite icon) {
        _profileIcon.sprite = icon;
    }

    //DUMMY
    //TODO: Decide how to get game results
    void Setup(int pictureCount, int differencesCount) {
        _infoHolder.DestroyAllChildren();
        for (int i = 0; i < pictureCount; i++) {
            var info = Instantiate(_pictureResultInfoPrefab, _infoHolder);
            var currentDifferencesCount = i == pictureCount - 1 ? Random.Range(1, differencesCount) : differencesCount;
            info.Setup(differencesCount, currentDifferencesCount, 5 * currentDifferencesCount);
        }
    }
}