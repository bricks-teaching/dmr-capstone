using UnityEngine;

/// <summary>
/// Global settings used across many files
/// </summary>
public static class Settings {
    public static float volume = 1f;
    public static float mouseSensitivity = 100f;
    public static MonoBehaviour cursorStateMutexOwner { get; private set; }
    public static bool sawTutorial = false;  // TODO move to state manager when implemented

    // ------------- keybinds ---------------
    public static string pauseKey = "escape";
    public static string interactKey = "f";
    public static string weaponOneKey = "1";
    public static string weaponTwoKey = "2";
    public static string hidePauseMenuKey = "backspace";

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
