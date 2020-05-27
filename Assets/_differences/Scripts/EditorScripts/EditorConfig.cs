using UnityEngine;
using UnityEngine.UI;

public class EditorConfig : MonoBehaviour {
   [SerializeField] Image _image1 = default;
   [SerializeField] Image _image2 = default;
   [SerializeField] DiffHandler _difHandlerPrefab = default;

   public Image Image1 => _image1;

   public Image Image2 => _image2;

   public DiffHandler DifHandlerPrefab => _difHandlerPrefab;
}
