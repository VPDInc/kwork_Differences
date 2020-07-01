using System.Collections;
using EasyMobile;
using UnityEngine;

public class FBShare : MonoBehaviour {
    [SerializeField] string _shareText = "Try to find differences!";

    public void Share() {
        StartCoroutine(SaveAndShareScreenshot());
    }
    
    // Coroutine that captures and saves a screenshot
    IEnumerator SaveAndShareScreenshot()
    {
        // Wait until the end of frame
        yield return new WaitForEndOfFrame();
        // The SaveScreenshot() method returns the path of the saved image
        // The provided file name will be added a ".png" extension automatically
        string path = Sharing.SaveScreenshot("screenshot");
        Sharing.ShareImage(path, _shareText);

    }

}
