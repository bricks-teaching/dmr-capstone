using UnityEngine;
using System.IO;

public static class Utils {
    /// <summary>
    /// Serialize an object to json and save it in a file
    /// </summary>
    /// <param name="values">object to serialize</param>
    /// <param name="filename">absolute path to file</param>
    public static void saveToJson(object values, string filename) {
        string json = JsonUtility.ToJson(values);
        File.WriteAllText(filename, json);
        Debug.Log($"Wrote to file {filename}");
    }

    /// <summary>
    /// Deserialize json text into an object and overwrite existing fields of
    /// the object. Any fields in the object not declared in the json are left
    /// as the defaults of the object.
    /// </summary>
    /// <param name="values">object to load deserialized data into</param>
    /// <param name="filename">absolute path to file</param>
    /// <param name="alreadyLoaded">optional variable to prevent reloading of value, should be tracked by the calling instance</param>
    /// <returns></returns>
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
