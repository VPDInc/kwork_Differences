using UnityEngine;
using UnityEngine.UI;

public class EditorConfig : MonoBehaviour {
   [SerializeField] Image _image1Hor = default;
   [SerializeField] Image _image2Hor = default;
   [SerializeField] Image _image1Vert = default;
   [SerializeField] Image _image2Vert = default;
   [SerializeField] DiffHandler _difHandlerPrefab = default;

   public (Image, Image) GetImages(Orientation orientation) {
      if (orientation == Orientation.Horizontal)
         return (_image1Hor, _image2Hor);
      return (_image1Vert, _image2Vert);
   }
   
   public DiffHandler DifHandlerPrefab => _difHandlerPrefab;
}
