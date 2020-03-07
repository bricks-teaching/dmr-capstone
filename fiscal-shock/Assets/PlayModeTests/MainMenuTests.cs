using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace PlayModeTests {
    public class MainMenuTests {
        [UnityTest]
        public IEnumerator testMainMenuPlayButton() {
            // load the menu
            SceneManager.LoadScene("Menu");
            // wait 10s for menu load
            yield return TestUtils.AssertSceneLoaded("Menu");
            // should be at the main menu
            Assert.AreEqual("Menu", SceneManager.GetActiveScene().name);
            // click the play button
            TestUtils.ClickButton("PlayButton");
            // should load the hub within 10s
            yield return TestUtils.AssertSceneLoaded("Hub");
            Assert.AreEqual("Hub", SceneManager.GetActiveScene().name);
        }

        /*[Test]
        public void testFail() { Assert.AreEqual(0, 1); }*/
    }
}
