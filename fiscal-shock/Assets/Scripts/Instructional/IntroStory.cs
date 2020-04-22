using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using FiscalShock.GUI;

public class IntroStory : MonoBehaviour {
    public List<GameObject> tutorialWeapons;
    public Inventory playerInventory;
    public TextMeshProUGUI storyLine1;
    public TextMeshProUGUI storyLine2;
    public GameObject player;
    private GameObject storyCamera;
    public List<GameObject> targets;
    public List<GameObject> bots;
    private string introStory1 = "";
    private string introStory2 = "";
    private bool part1 = true;
    private int storyPosition = 0;
    private int targetsHit;
    private int delay = 0;
    public float textSpeed;
    public GameObject milkman;
    private string[] storiesTop = {"A small loan has gotten out of hand, and now",
                                "If you manage your money right, you should",
                                "Luckily, there is a call to arms. (You’ll need your",
                                "The evil CEO of BOTCORP has started ravaging",
                                "Only desperate... ",
                                "Yes, just him, as the CEO of BOTCORP is the",
                                "So, are you up for the challenge?",
                                "I can loan you some equipment.",
                                "",
                                "Great job! Now, let's try out a bigger weapon.",
                                "You clearly have some milk-flinging skills,",
                                "You get money for every BOTCORP bot you take",
                                "To protect your family, you must assume a new identity.",
                                "Your codename is <i>Elegant Mess</i>. We expect great <s>profit</s> things from you."};
    private string[] storiesBottom = { "you must rebuild your credit and get out of debt.",
                                "get out of all this just fine.",
                                "legs too, heh heh).",
                                "archaeological and natural settings for loot.",
                                "citizens can stop him.",
                                "only living human employed there.",
                                "Follow me... I wanna test your skills.",
                                "(You break it, you buy it!)",
                                "<i>Press the Left Mouse Button to fire your weapon.</i>",
                                "<i>Press 2 or use the scroll wheel.</i>",
                                "so here's our deal...",
                                "down, you can use it to get yourself out of debt.",
                                "",
                                ""};

    private float showing = 0;
    private int animationFrame = 1;
    private int animationState = 0;
    private bool weaponActive = false;
    private bool weaponSwitched = false;
    private Vector3 rotateFrom;
    private Vector3 rotateCameraFrom;
    private Vector3 stareAtMilkman = new Vector3(-75.06f, -4.36f, 114.66f);
    private PlayerShoot playerShoot;
    private MouseLook mouseLook;

    void Start() {
        storyCamera = GameObject.FindGameObjectWithTag("MainCamera");
        mouseLook = storyCamera.GetComponent<MouseLook>();
        playerShoot = player.GetComponentInChildren<PlayerShoot>();
        player.GetComponentInChildren<PlayerMovement>().enabled = false;

        StateManager.inStoryTutorial = true;

        playerShoot.enabled = false;
        introStory1 = storiesTop[storyPosition];
        introStory2 = storiesBottom[storyPosition];
        Time.timeScale = 0;
        mouseLook.enabled = false;
        foreach (GameObject weapon in tutorialWeapons) {
            GameObject realWeapon = Instantiate(weapon);
            playerInventory.addWeapon(realWeapon);
        }
        foreach (GameObject target in targets) {
            TargetBehavior behaviorScript = target.GetComponent<TargetBehavior>();
            behaviorScript.story = this;
        }
    }

    // Update is called once per frame
    void Update() {
        showText();
        if (storyPosition == 6 && !part1 && (int) showing > introStory2.Length) {
            animationState = 1;
        }
        else if (storyPosition == 8 && !part1 && (int) showing > introStory2.Length && animationState == 1) {
            animationState = 2;
            animationFrame = 0;
            weaponActive = true;
        }

        if (part1 && storyPosition == 4 && (int) showing == introStory1.Length) {
            if (delay < 12) {
                showing -= textSpeed;
                delay++;
            }
            else if (delay == 12) {
                showing += 7;
                delay++;
                introStory1 = "Only <s>desperate</s>...  disposable... ";
            }
            else if (delay < 24) {
                showing -= textSpeed;
                delay++;
            }
            else {
                showing += 7;
                introStory1 = "Only <s>desperate</s>...  <s>disposable</s>...  noble and brave ";
            }
        }

        if (part1 && (int) showing > introStory1.Length) {
            part1 = false;
            showing = 0;
        }
        if (Input.GetMouseButtonDown(0) && storyPosition == 8 && animationFrame > 34) {
            storyLine1.text = "";
            storyLine2.text = "";
        }
        if (Input.GetMouseButtonDown(0) && !weaponActive) {
            if (part1 || (int) showing <= introStory2.Length || (storyPosition == 6 && animationFrame < 75)) {
                if (storyPosition == 4) {
                    introStory1 = "Only <s>desperate</s>...  <s>disposable</s>...  noble and brave ";
                }
                storyLine1.text = introStory1;
                part1 = false;
                showing = introStory2.Length - 2 * textSpeed;
                showText();
                if (storyPosition == 6) {
                    animationFrame = 75;
                    milkman.transform.position = new Vector3(-87.06f, -5.27f, 112.836f);
                    milkman.transform.rotation = Quaternion.Euler(new Vector3(0, 105.1f, 0));
                    player.transform.position = new Vector3(-83.8f, -4.34f, 112.92f);
                    player.transform.rotation = Quaternion.Euler(new Vector3(1.47f, -100f, 0f));
                }
                else if (storyPosition == 8) {
                    animationFrame = 30;
                    animationState = 2;
                    weaponActive = true;
                    // Debug.Log(animationState + " ---  " + animationFrame);
                    player.transform.position = new Vector3(-91.3f, -4.34f, 107.91f);
                }
            }
            else {
                if (storyPosition == 9) {
                    if (weaponSwitched) {
                        weaponActive = true;
                        storyLine1.text = "";
                        storyLine2.text = "";
                    }
                }
                else if (storyPosition < storiesTop.Length - 1) {
                    storyPosition++;
                    if (storyPosition == 3) {
                        player.transform.position = new Vector3(40, 7.7f, 88);
                        Time.timeScale = 0.68f;
                    }
                    else if (storyPosition == 4) {
                        foreach (GameObject bot in bots) {
                            EnemyShoot botScript = bot.GetComponent<EnemyShoot>();
                            botScript.enabled = false;
                        }
                        player.transform.position = stareAtMilkman;
                        Time.timeScale = 1;
                    }
                    introStory1 = storiesTop[storyPosition];
                    introStory2 = storiesBottom[storyPosition];
                    showing = 0;
                    part1 = true;
                    storyLine2.text = "";
                }
                else {
                    StateManager.resetToDefaultState();
                    Settings.values.sawStoryTutorial = true;
                    Time.timeScale = 1;
                    SceneManager.LoadScene("Hub");
                }
            }
        }

        if (playerShoot.slot == 1) {
            if (animationState == 4) {
                foreach (GameObject target in targets) {
                    TargetBehavior behaviorScript = target.GetComponent<TargetBehavior>();
                    behaviorScript.activateTarget();
                }
                weaponSwitched = true;
                animationState++;
            }
        }
        if (animationState == 1) {
            if (animationFrame < 7) {
                milkman.transform.position += new Vector3(0, 0, 0.038f);
                milkman.transform.rotation = Quaternion.Euler(new Vector3(0, 144 + 19.45f * animationFrame, 0));
                animationFrame++;
            }
            else if (animationFrame < 54) {
                milkman.transform.position += new Vector3(-0.25f, 0, -0.056f);
                animationFrame++;
            }
            else if (animationFrame < 63) {
                milkman.transform.rotation = Quaternion.Euler(new Vector3(0, 144 + 19.45f * (7 - (animationFrame - 53)), 0));
            }
            if (animationFrame > 16 && animationFrame < 39) {
                player.transform.rotation = Quaternion.Euler(new Vector3(1.47f, 14 - 3f * animationFrame, 0));
            }
            else if (animationFrame < 54) {
                player.transform.position += new Vector3(-0.25f, 0, -0.056f);
            }
            else if (animationFrame < 74) {
                player.transform.position += new Vector3(-0.25f, 0, -0.056f);
                animationFrame++;
            }
        }
        else if (animationState == 2) {
            if (animationFrame < 30) {
                player.transform.position += new Vector3(-0.25f, 0, -0.167f);
                animationFrame++;
            }
            else if (animationFrame < 36) {
                animationFrame++;
            }
            else {
                milkman.transform.rotation = Quaternion.Euler(new Vector3(9f, 216f, 0));
                playerShoot.enabled = true;
                player.GetComponentInChildren<Light>().intensity = 0.5f;
                mouseLook.enabled = true;
                foreach (GameObject target in targets) {
                    TargetBehavior behaviorScript = target.GetComponent<TargetBehavior>();
                    behaviorScript.activateTarget();
                }
                animationState++;
            }
        }
        else if (animationState == 6) {
            if (animationFrame > 10 && animationFrame < 19) {
                float change = (animationFrame - 10) * 4.375f;
                milkman.transform.position += new Vector3(-0.53f, 0, -0.542f);
                milkman.transform.rotation = Quaternion.Euler(new Vector3(0, 216f - change, 0));
            }
            if (animationFrame < 15) {
                float remainder = 14 - animationFrame;
                player.transform.rotation = Quaternion.Euler(new Vector3(rotateFrom.x * remainder / 14, rotateFrom.y * remainder / 14, rotateFrom.z * remainder / 14));
                storyCamera.transform.rotation = Quaternion.Euler(rotateCameraFrom * remainder / 14);
                animationFrame++;
            }
            else if (animationFrame < 19) {
                animationFrame++;
            }
        }
    }

    public void hitTarget() {
        targetsHit++;
        if (targetsHit == 6) {
            storyPosition++;
            introStory1 = storiesTop[storyPosition];
            introStory2 = storiesBottom[storyPosition];
            showing = 0;
            part1 = true;
            storyLine2.text = "";
            animationState++;
            animationFrame = 0;
            weaponActive = false;
            targetsHit = 0;
            if (animationState == 6) {
                rotateFrom = player.transform.rotation.eulerAngles;
                rotateCameraFrom = storyCamera.transform.rotation.eulerAngles;
                if (rotateFrom.z > 180) {
                    rotateFrom.z -= 360;
                }
                if (rotateCameraFrom.z > 180) {
                    rotateCameraFrom.z -= 360;
                }
                if (rotateFrom.x > 180) {
                    rotateFrom.x -= 360;
                }
                if (rotateCameraFrom.x > 180) {
                    rotateCameraFrom.x -= 360;
                }
                if (rotateFrom.y > 180) {
                    rotateFrom.y -= 360;
                }
                if (rotateCameraFrom.y > 180) {
                    rotateCameraFrom.y -= 360;
                }
                mouseLook.enabled = false;
                playerShoot.enabled = false;
            }
        }
    }

    void showText() {
        if (showing < 0) {
            showing = 0;  // clicking too fast causes errors
        }
        if (part1) {
            if ((int) showing <= introStory1.Length) {
                storyLine1.text = introStory1.Substring(0, (int) showing);
            }
        }
        else {
            if ((int) showing <= introStory2.Length) {
                storyLine2.text = introStory2.Substring(0, (int) showing);
            }
        }
        showing += textSpeed;
    }
}
