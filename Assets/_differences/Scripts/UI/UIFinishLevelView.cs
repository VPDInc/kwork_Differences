using System.Linq;

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
    public void Show(int levelNum, LevelResultInfo levelResultInfo, int coinReward) {
        Show();
        SetLevelName(levelNum);
        SetCoinsAmount(coinReward);
        Setup(levelResultInfo);
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
    void Setup(LevelResultInfo levelResultInfo) {
        _infoHolder.DestroyAllChildren();

        var pictureCount = levelResultInfo.PicturesCount;
        var differencesCount = levelResultInfo.DifferencesCount;
        
        for (int i = 0; i < pictureCount; i++) {
            var info = Instantiate(_pictureResultInfoPrefab, _infoHolder);
            var differencePoints = levelResultInfo.PictureResults[i].DifferencePoints;
            var guessedDifferences = differencePoints.Count(x => x.IsOpen);
            //TODO: Implement ranking score
            info.Setup(differencePoints, 5 * guessedDifferences);
        }
    }
}