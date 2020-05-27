using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Airion/New Level")]
public class Level : ScriptableObject {
    public GameObject LevelObject => _levelObject;
    public string Id => _id;
    
    [SerializeField] string _id = "empty"; 
    [SerializeField] GameObject _levelObject = default;
}
