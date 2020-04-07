using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroStory : MonoBehaviour
{
    private TextMeshProUGUI storyLine1;
    private TextMeshProUGUI storyLine2;
    public GameObject storyCamera;
    private string introStory1 = "";
    private string introStory2 = "";
    private bool part1 = true;
    private int storyPosition = 0;
    private int delay = 0;
    public float textSpeed;
    private string spaces = "                                                                                                                         ";
    private string[] storiesTop = {"A small loan has gotten out of hand, and now",
                                "If you manage your money right, you should",
                                "Luckily, there is a call to arms (You’ll need your",
                                "The evil BOTCORP has started ravaging",
                                "Only desperate.. ",
                                "Yes just him, as the CEO of BOTCORP is the",
                                "We know you have some milk-flinging skills,",
                                "You get money for every BOTCORP bot you take",
                                "To protect your family we changed your name."};
    private string[] storiesBottom = { "you must rebuild your credit and get out of debt.", 
                                "get out of all this just fine.", 
                                "legs too heh heh).", 
                                "temples and natural settings for loot.", 
                                "citizens can stop him.", 
                                "only living human employed there.", 
                                "so here's our deal...", 
                                "down, you can use it to get yourself out of debt.",
                                "You will now be known as Elegant Mess."};

    private float showing = 0;
    // Start is called before the first frame update
    void Start()
    {
        storyLine1 = GetComponentsInChildren<TextMeshProUGUI>()[0];
        storyLine2 = GetComponentsInChildren<TextMeshProUGUI>()[1];
        introStory1 = storiesTop[storyPosition];
        introStory2 = storiesBottom[storyPosition];
        Time.timeScale = 0;
        int fontHeight = Screen.width / 23;
        storyLine1.fontSize = fontHeight;
        storyLine2.fontSize = fontHeight;
        RectTransform rt = storyLine1.GetComponent (typeof (RectTransform)) as RectTransform;
        rt.sizeDelta = new Vector2 (0, fontHeight * 5.5f);
        rt = storyLine2.GetComponent (typeof (RectTransform)) as RectTransform;
        rt.sizeDelta = new Vector2 (0, fontHeight * 3);
    }

    // Update is called once per frame
    void Update()
    {
        showText();

        if(part1 && storyPosition == 4 && (int) showing == introStory1.Length){
            if(delay < 12){
                showing -= textSpeed;
                delay++;
            } else if(delay == 12){
                showing += 7;
                delay++;
                introStory1 = "Only <s>desperate</s>..  disposable... ";
            } else if(delay < 24){
                showing -= textSpeed;
                delay++;
            } else {
                showing += 7;
                introStory1 = "Only <s>desperate</s>..  <s>disposable</s>...  noble and brave ";
            }
        }

        if(part1 && (int) showing > introStory1.Length){
            part1 = false;
            showing = 0;
        }

        if(Input.GetMouseButtonDown(0)){
            if(part1 || (int) showing <= introStory2.Length){
                 if(storyPosition == 4){
                     introStory1 = "Only <s>desperate</s>..  <s>disposable</s>...  noble and brave ";
                }
                storyLine1.text = introStory1;
                part1 = false;
                showing = introStory2.Length - 2 * textSpeed;
                showText();
            } else {
                if(storyPosition < storiesTop.Length - 1){
                    storyPosition++;
                    if(storyPosition == 3){
                        storyCamera.transform.position = new Vector3(40,7.7f,88);
                        Time.timeScale = 0.68f;
                    } else {
                        storyCamera.transform.position = new Vector3(-75,-1,115);
                        Time.timeScale = 0;
                    }
                    introStory1 = storiesTop[storyPosition];
                    introStory2 = storiesBottom[storyPosition];
                    showing = 0;
                    part1 = true;
                    storyLine2.text = "";
                } else {
                    Time.timeScale = 1;
                    SceneManager.LoadScene("Hub");
                }
            }
        }
    }

    void showText(){
        if(part1){
            if ((int) showing <= introStory1.Length){
                storyLine1.text = introStory1.Substring(0,(int) showing);
            }
        }else {
            if ((int) showing <= introStory2.Length){
                storyLine2.text = introStory2.Substring(0,(int) showing);
            }
        }
        showing += textSpeed;
        
    }
}
