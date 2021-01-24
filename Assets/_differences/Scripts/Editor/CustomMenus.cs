using System.IO;
using UnityEditor;
using UnityEngine;

namespace _differences.Scripts.Editor
{
    public static class CustomMenus 
    {
        private const string SAVE_DATA_PATH = "data.dat";

        [MenuItem("Window/Differences/RemoveSaveData")]
        public static void RemoveSevedData()
        {
            var _dataPath = Path.Combine(Application.persistentDataPath, SAVE_DATA_PATH);

            try
            {
                PlayerPrefs.DeleteAll();

                if (File.Exists(_dataPath))
                    File.Delete(_dataPath);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            Debug.Log("<color=green>Success removed</color>");
        }
    }
}