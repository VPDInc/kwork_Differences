using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CloudScriptsExtensions
{
    const string CLOUD_SCRIPT_DIRECOTRY = "CloudScripts";
    const string CLOUD_SCRIPT_SOURCES_DIRECTORY = "Sources";
    const string CLOUD_SCRIPT_RESULT_DIRECTORY = "_JoinResult";
    const string CLOUD_SCRIPT_RESULT_FILE = "CloudScripts.js";

    [MenuItem("Differences/Cloud Scripts/Join Scripts")]
    public static void JoinScripts()
    {
        string rootPath = Directory.GetParent(Application.dataPath).FullName;
        string folderPath = Path.Combine(rootPath, CLOUD_SCRIPT_DIRECOTRY, CLOUD_SCRIPT_SOURCES_DIRECTORY);
        string resultPath = Path.Combine(rootPath, CLOUD_SCRIPT_DIRECOTRY, CLOUD_SCRIPT_RESULT_DIRECTORY);

        JoinScripts(folderPath, resultPath);
    }

    private static void JoinScripts(string folderPath, string resultFolder)
    {
        var resultUploadScript = new StringBuilder();

        string[] scriptPaths = Directory.GetFiles(folderPath, "*.js");

        foreach (var scriptPath in scriptPaths)
        {
            resultUploadScript.AppendLine(File.ReadAllText(scriptPath));
        }

        if (!Directory.Exists(resultFolder))
            Directory.CreateDirectory(resultFolder);

        File.WriteAllText(Path.Combine(resultFolder, CLOUD_SCRIPT_RESULT_FILE), resultUploadScript.ToString());

        Debug.Log("<color=green>Success</color>");
    }
}
