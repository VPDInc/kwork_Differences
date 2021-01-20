using UnityEditor;
using UnityEngine;

namespace _differences.Scripts.Editor
{
    public static class CustomMenus 
    {
        [MenuItem("Window/Differences/RemoveSaveData")]
        public static void RemoveSevedData()
        {
            try
            {
                PlayerPrefs.DeleteAll();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            Debug.Log("<color=green>Success removed</color>");
        }
    }
}