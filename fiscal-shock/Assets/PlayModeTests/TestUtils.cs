#pragma warning disable
/* https://medium.com/lonely-vertex-development/automated-tests-in-unity-with-examples-a2de2361ae3c */
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using NUnit.Framework;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class WaitForSceneLoaded : CustomYieldInstruction
{
    readonly string sceneName;
    readonly float timeout;
    readonly float startTime;
    bool timedOut;

    public bool TimedOut => timedOut;

    public override bool keepWaiting {
        get {
            var scene = SceneManager.GetSceneByName(sceneName);
            var sceneLoaded = scene.IsValid() && scene.isLoaded;

            if (Time.realtimeSinceStartup - startTime >= timeout) {
                timedOut = true;
            }

            return !sceneLoaded && !timedOut;
        }
    }

    public WaitForSceneLoaded(string newSceneName, float newTimeout = 10)
    {
        sceneName = newSceneName;
        timeout = newTimeout;
        startTime = Time.realtimeSinceStartup;
    }
}

public class TestUtils
{
    public static void ClickButton(string name)
    {
        // Find button Game Object
        var go = GameObject.Find(name);
        Assert.IsNotNull(go, "Missing button " + name);

        // Set it selected for the Event System
        EventSystem.current.SetSelectedGameObject(go);

        // Invoke click
        go.GetComponent<Button>().onClick.Invoke();
    }

    public static IEnumerator AssertSceneLoaded(string sceneName)
    {
        var waitForScene = new WaitForSceneLoaded(sceneName);
        yield return waitForScene;
        Assert.IsFalse(waitForScene.TimedOut, "Scene " + sceneName + " was never loaded");
    }
}
