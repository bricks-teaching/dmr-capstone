using UnityEngine;
using System.IO;

public static class Utils {
    public static void saveToJson(object values, string filename) {
        string json = JsonUtility.ToJson(values);
        File.WriteAllText(filename, json);
        Debug.Log($"Wrote to file {filename}");
    }

    public static bool loadFromJson(object values, string filename, bool alreadyLoaded = false) {
        if (alreadyLoaded || !File.Exists(filename)) {
             return alreadyLoaded;
        }
        string json = File.ReadAllText(filename);
        JsonUtility.FromJsonOverwrite(json, values);
        Debug.Log($"Loaded from file {filename}");
        return true;
    }

}
