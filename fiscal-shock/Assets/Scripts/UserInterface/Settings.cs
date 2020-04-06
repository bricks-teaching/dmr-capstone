using UnityEngine;
using System.Collections.Generic;

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
        QualitySettings.SetQualityLevel(values.currentQuality, true);
        Application.targetFrameRate = values.targetFramerate;
        Screen.SetResolution(values.resolutionWidth, values.resolutionHeight, values.fullscreen);

        if (values.overrideQualitySettings) {
            QualitySettings.vSyncCount = values.vsyncCount;

            // Texture quality
            QualitySettings.anisotropicFiltering = values.anisotropicTextures;
            QualitySettings.antiAliasing = values.antialiasingSamples;

            // Lighting
            QualitySettings.pixelLightCount = values.pixelLightCount;
            QualitySettings.shadowDistance = values.shadowDistance;
            QualitySettings.shadowResolution = values.shadowResolution;
        } else {
            QualitySettings.vSyncCount = qualityPreset.vsyncCount;

            // Texture quality
            QualitySettings.anisotropicFiltering = qualityPreset.anisotropicTextures;
            QualitySettings.antiAliasing = qualityPreset.antialiasingSamples;

            // Lighting
            QualitySettings.pixelLightCount = qualityPreset.pixelLightCount;
            QualitySettings.shadowDistance = qualityPreset.shadowDistance;
            QualitySettings.shadowResolution = qualityPreset.shadowResolution;
        }
    }

    public static void resetToCurrentQualityDefaults() {
        values.overrideQualitySettings = false;
        updateCurrentSettings();

        // Frame rate
        values.vsyncCount = QualitySettings.vSyncCount;
        values.targetFramerate = Application.targetFrameRate;

        // Texture quality
        values.anisotropicTextures = QualitySettings.anisotropicFiltering;
        values.antialiasingSamples = QualitySettings.antiAliasing;

        // Lighting
        values.pixelLightCount = QualitySettings.pixelLightCount;
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

    public static QualityPreset qualityPreset = DefaultQualitySettings.Default;

    /* stringified */
    public static string[] shadowResNames = {
        "Off", // warning: special case
        "Low",
        "Medium",
        "High",
        "Very High"
    };

    public static string[] anisotropicNames = {
        "Disabled",
        "Enabled",
        "Force Enabled"
    };

    public static string[] pixelQualityNames = {
        "None",
        "Low",
        "Medium",
        "High",
        "Ultra"
    };

    public static string[] vsyncCountNames = {
        "Disabled",
        "Every",
        "Every Other"
    };

    public static string[] antialiasingNames = {
        "Disabled",
        "2x MSAA",
        "4x MSAA",
        "8x MSAA"
    };

    public static Resolution getResolutionByIndex(int i) {
        return Screen.resolutions[i];
    }

    public static List<string> getSupportedResolutions() {
        List<string> res = new List<string>();
        foreach (Resolution r in Screen.resolutions) {
            res.Add($"{r.width}x{r.height}");
        }
        return res;
    }
}

/// <summary>
/// Unity has no concept of "reset to the default of this quality preset,"
/// so here we go...
/// </summary>
public static class DefaultQualitySettings {
    public static QualityPreset VeryLow = new QualityPreset {
        vsyncCount = 0,
        anisotropicTextures = AnisotropicFiltering.Disable,
        antialiasingSamples = 0,
        pixelLightCount = 0,
        shadowDistance = 0,
        shadowResolution = ShadowResolution.Low
    };
    public static QualityPreset Low = new QualityPreset {
        vsyncCount = 0,
        anisotropicTextures = AnisotropicFiltering.Disable,
        antialiasingSamples = 0,
        pixelLightCount = 1,
        shadowDistance = 0,
        shadowResolution = ShadowResolution.Low
    };
    public static QualityPreset Medium = new QualityPreset {
        vsyncCount = 0,
        anisotropicTextures = AnisotropicFiltering.Enable,
        antialiasingSamples = 0,
        pixelLightCount = 1,
        shadowDistance = 20,
        shadowResolution = ShadowResolution.Low
    };
    public static QualityPreset Default = new QualityPreset {
        vsyncCount = 1,
        anisotropicTextures = AnisotropicFiltering.Enable,
        antialiasingSamples = 2,
        pixelLightCount = 1,
        shadowDistance = 40,
        shadowResolution = ShadowResolution.Low
    };
    public static QualityPreset High = new QualityPreset {
        vsyncCount = 1,
        anisotropicTextures = AnisotropicFiltering.Enable,
        antialiasingSamples = 2,
        pixelLightCount = 2,
        shadowDistance = 40,
        shadowResolution = ShadowResolution.Medium
    };
    public static QualityPreset VeryHigh = new QualityPreset {
        vsyncCount = 1,
        anisotropicTextures = AnisotropicFiltering.Enable,
        antialiasingSamples = 4,
        pixelLightCount = 3,
        shadowDistance = 70,
        shadowResolution = ShadowResolution.High
    };
    public static QualityPreset Ultra = new QualityPreset {
        vsyncCount = 1,
        anisotropicTextures = AnisotropicFiltering.Enable,
        antialiasingSamples = 4,
        pixelLightCount = 4,
        shadowDistance = 128,
        shadowResolution = ShadowResolution.VeryHigh
    };

    public static QualityPreset getPresetByIndex(int i) {
        switch (i) {
            case 0:
                return VeryLow;
            case 1:
                return Low;
            case 2:
                return Medium;
            case 3:
                return Default;
            case 4:
                return High;
            case 5:
                return VeryHigh;
            default:
                return Ultra;
        }
    }
}

/// <summary>
/// Holds values of quality preset defaults. Unity does not make
/// quality preset values available at runtime, and any manually
/// modified settings are never reset.
/// </summary>
public struct QualityPreset {
    public int vsyncCount;
    public AnisotropicFiltering anisotropicTextures;
    public int antialiasingSamples;
    public int pixelLightCount;
    public float shadowDistance;
    public ShadowResolution shadowResolution;
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

    public bool showFPS = false;

    // --- configurable graphics ---
    public bool overrideQualitySettings = false;
    public int targetFramerate = 60;  // requires vsyncCount = 0
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public bool fullscreen = false;
    public int vsyncCount = 1;  // 0, 1, 2, 3, 4
    public AnisotropicFiltering anisotropicTextures = AnisotropicFiltering.Disable;  // Disable, Enable, ForceEnable
    public int antialiasingSamples = 2;  // 0, 2, 4, 8 only supported
    public int pixelLightCount = 1;  // 0, 1, 2, 3, 4; 0 = dark, no pixel lights!
    public float shadowDistance = 40;  // maximum distance to draw shadows at
    public ShadowResolution shadowResolution = ShadowResolution.Low;  // Low, Medium, High, VeryHigh

    /* Very Low, Low, Medium, [Default], High, Very High, Ultra */
    /* Save the strings so they're available in the settings.json for users
       who break stuff and want to reset to defaults.
    */
    public string[] qualityLevelNames = QualitySettings.names;
    public int currentQuality = 3;
    public string currentQualityName = "Default";
}
