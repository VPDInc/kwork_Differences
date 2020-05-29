using UnityEngine;

public class Database : MonoBehaviour {
    [SerializeField] TextAsset _debugLevel = default;

    public Data GetLevelByNum(int levelNum) {
        var data = DiffUtils.Parse(_debugLevel.text);
        return data;
    }
}
