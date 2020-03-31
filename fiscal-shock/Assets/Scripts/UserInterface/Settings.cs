using UnityEngine;

/// <summary>
/// Global settings used across many files
/// </summary>
public static class Settings {
    public static MonoBehaviour cursorStateMutexOwner { get; private set; }
    public static SettingsValues values = new SettingsValues();
    private static bool alreadyLoadedSettings = false;
    public static float volume {
        get => values.volume;
        set => values.volume = value;
    }
    public static float mouseSensitivity {
        get => values.mouseSensitivity;
        set => values.mouseSensitivity = value;
    }

    // ------------- keybinds ---------------
    public static string pauseKey => values.pauseKey;
    public static string interactKey => values.interactKey;
    public static string weaponOneKey => values.weaponOneKey;
    public static string weaponTwoKey => values.weaponTwoKey;
    public static string hidePauseMenuKey => values.hidePauseMenuKey;

    // occlusion culling
    public static int occlusionSampleDelay => values.occlusionSampleDelay;
    public static int occlusionSamples => values.occlusionSamples;
    public static int occlusionHideDelay => values.occlusionHideDelay;
    public static int viewDistance => values.viewDistance;
    public static int minOcclusionDistance => values.minOcclusionDistance;

    // max at 60 fps when starting from menu scene only
    public static int targetFramerate => values.targetFramerate;
    public static int vsync => values.vsyncEnabled;

    private static readonly string settingsFilename = Application.persistentDataPath + "/settings.json";

    public static void saveSettings() {
        Utils.saveToJson(values, settingsFilename);
    }

    public static void loadSettings() {
        alreadyLoadedSettings = Utils.loadFromJson(values, settingsFilename, alreadyLoadedSettings);
    }

    /// <summary>
    /// Get the mutex on the cursor state while locking the cursor
    ///
    /// <para>Remember to free it with unlockCursorState so the mutex is released!</para>
    /// </summary>
    /// <param name="caller"></param>
    /// <returns></returns>
    public static bool mutexLockCursorState(MonoBehaviour caller) {
        bool success = lockCursorState(caller);
        if (success) {
            cursorStateMutexOwner = caller;
        }
        return success;
    }

    /// <summary>
    /// Get the mutex on the cursor state while freeing the cursor
    ///
    /// <para>Remember to free it with lockCursorState so the mutex is released!</para>
    /// </summary>
    /// </summary>
    /// <param name="caller"></param>
    /// <returns></returns>
    public static bool mutexUnlockCursorState(MonoBehaviour caller) {
        bool success = unlockCursorState(caller);
        if (success) {
            cursorStateMutexOwner = caller;
        }
        return success;
    }

    /// <summary>
    /// Attempts to lock the cursor state and disowns the mutex if the caller already owned it
    /// </summary>
    /// <param name="caller"></param>
    /// <returns></returns>
    public static bool lockCursorState(MonoBehaviour caller) {
        if (cursorStateMutexOwner == null || cursorStateMutexOwner == caller) {
            Cursor.lockState = CursorLockMode.Locked;
            cursorStateMutexOwner = null;
            return true;
        } else {
            Debug.LogWarning($"{caller} attempted to lock cursor state without owning the mutex!");
            return false;
        }
    }

    /// <summary>
    /// Attempts to unlock the cursor state and disowns the mutex if the caller already owned it
    /// </summary>
    /// <param name="caller"></param>
    /// <returns></returns>
    public static bool unlockCursorState(MonoBehaviour caller) {
        if (cursorStateMutexOwner == null || cursorStateMutexOwner == caller) {
            Cursor.lockState = CursorLockMode.None;
            cursorStateMutexOwner = null;
            return true;
        } else {
            Debug.LogWarning($"{caller} attempted to unlock cursor state without owning the mutex!");
            return false;
        }
    }

    /// <summary>
    /// Violently unlocks the cursor state and evicts the existing mutex owner, if it was owned.
    ///
    /// <para>Do not use except when you NEED to impolitely alter controls, i.e., changing scenes.</para>
    /// </summary>
    public static void forceUnlockCursorState() {
        cursorStateMutexOwner = null;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Violently locks the cursor state and evicts the existing mutex owner, if it was owned.
    ///
    /// <para>Do not use except when you NEED to impolitely alter controls, i.e., changing scenes.</para>
    /// </summary>
    public static void forceLockCursorState() {
        cursorStateMutexOwner = null;
        Cursor.lockState = CursorLockMode.Locked;
    }
}

/// <summary>
/// Serializable data class that can be saved to json.
/// </summary>
[System.Serializable]
public class SettingsValues {
    public float volume = 0.5f;
    public float mouseSensitivity = 100f;

    // ------------- keybinds ---------------
    public string pauseKey = "escape";
    public string interactKey = "f";
    public string weaponOneKey = "1";
    public string weaponTwoKey = "2";
    public string hidePauseMenuKey = "backspace";

    // occlusion culling
    public int occlusionSampleDelay = 128;
    public int occlusionSamples = 128;
    public int occlusionHideDelay = 32;
    public int viewDistance = 128;
    public int minOcclusionDistance = 32;

    // max at 60 fps when starting from menu scene only
    public int targetFramerate = 60;
    public int vsyncEnabled = 0;
}
