using Assets.CoreFramework.Scripts.Managers.Dialouge.Core;
using System.IO;
using UnityEngine;

public static class DialogueLoader
{
    public static DialogueData LoadDialogue(string fileName)
    {
        // Paksa ekstensi .json
        if (!fileName.EndsWith(".json"))
            fileName += ".json";

        string fullPath = Path.Combine(Application.streamingAssetsPath, fileName);

        // Normalize path (Windows suka drama)
        fullPath = fullPath.Replace("\\", "/");

        if (!File.Exists(fullPath))
        {
            Debug.LogError(
                $"[DialogueLoader] File NOT FOUND\n" +
                $"Expected path:\n{fullPath}"
            );
            return null;
        }

        string json = File.ReadAllText(fullPath);
        return JsonUtility.FromJson<DialogueData>(json);
    }
}
