using UnityEngine;

/// <summary>
/// Global settings used across many files
/// </summary>
public static class Settings {
    public static SettingsValues values = new SettingsValues();
    public static MonoBehaviour cursorStateMutexOwner { get; private set; }
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

    private static readonly string settingsFilename = Application.persistentDataPath + "/settings.json";

    public static void saveSettings() {
        Utils.saveToJson(values, settingsFilename);
    }

    public static void loadSettings() {
        Utils.loadFromJson(values, settingsFilename);
        updateCurrentSettings();
    }

    public static void updateCurrentSettings() {
        // Apply default quality level settings first
        QualitySettings.SetQualityLevel(values.currentQuality);

        if (values.overrideQualitySettings) {
            // Frame rate
            QualitySettings.vSyncCount = values.vsyncCount;
            Application.targetFrameRate = values.targetFramerate;

            // Particle effects
            QualitySettings.softParticles = values.enableSoftParticles;

            // Texture quality
            QualitySettings.anisotropicFiltering = values.anisotropicTextures;
            QualitySettings.antiAliasing = values.antialiasingSamples;

            // Lighting
            QualitySettings.pixelLightCount = (int)values.pixelLightCount;
            QualitySettings.shadowDistance = values.shadowDistance;
            QualitySettings.shadowResolution = values.shadowResolution;
        }
    }

    public static void resetToCurrentQualityDefaults() {
        QualitySettings.SetQualityLevel(values.currentQuality, true);

        // Frame rate
        values.vsyncCount = QualitySettings.vSyncCount;
        values.targetFramerate = Application.targetFrameRate;

        // Particle effects
        values.enableSoftParticles = QualitySettings.softParticles;

        // Texture quality
        values.anisotropicTextures = QualitySettings.anisotropicFiltering;
        values.antialiasingSamples = QualitySettings.antiAliasing;

        // Lighting
        values.pixelLightCount = (PixelLightQuality)QualitySettings.pixelLightCount;
        values.shadowDistance = QualitySettings.shadowDistance;
        values.shadowResolution = QualitySettings.shadowResolution;
    }

    public static void quitToDesktop() {
        saveSettings();
        Application.Quit();
    }

    public static void quitToMainMenu() {
        LoadingScreen loading = GameObject.Find("LoadingScreen")?.GetComponentInChildren<LoadingScreen>();

        // Destroy singletons; tracked by StateManager
        // Caution: don't add the load camera to the list!
        foreach (GameObject go in StateManager.singletons) {
            if (go != null) {  // just in case somebody else destroyed it...
                UnityEngine.Object.Destroy(go);
            }
        }
        StateManager.singletons.Clear();  // empty the list
        saveSettings();

        // Load the right scene, using the loading screen if it is available
        if (loading != null) {
            loading.startLoadingScreen("Menu");
        } else {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
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

    // --- configurable graphics ---
    public int targetFramerate = 60;  // requires vsyncCount = 0
    public int vsyncCount = 1;  // 0, 1, 2, 3, 4
    public bool enableSoftParticles = false;
    public AnisotropicFiltering anisotropicTextures = AnisotropicFiltering.Disable;  // Disable, Enable, ForceEnable
    public int antialiasingSamples = 2;  // 0, 2, 4, 8 only supported
    public PixelLightQuality pixelLightCount = PixelLightQuality.Low;  // 0, 1, 2, 3, 4; 0 = dark, no pixel lights!
    public float shadowDistance = 40;  // maximum distance to draw shadows at
    public ShadowResolution shadowResolution = ShadowResolution.Low;  // Low, Medium, High, VeryHigh

    /* Very Low, Low, Medium, [Default], High, Very High, Ultra */
    /* Save the strings so they're available in the settings.json for users
       who break stuff and want to reset to defaults.
    */
    public string[] qualityLevelNames = QualitySettings.names;
    public bool overrideQualitySettings = true;
    public string currentQualityName = "Default";
    public int currentQuality = 3;
}

public enum PixelLightQuality {
    None,
    Low,
    Medium,
    High,
    Ultra
}
