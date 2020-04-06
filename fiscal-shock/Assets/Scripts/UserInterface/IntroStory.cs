using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroStory : MonoBehaviour
{
    private TextMeshProUGUI storyText;
    private TextMeshProUGUI strikeThrough1;
    private TextMeshProUGUI strikeThrough2;
    public GameObject storyCamera;
    private string introStory = "";
    private int storyPosition = 0;
    private int delay1 = 0;
    private int delay2 = 0;
    public float textSpeed;

    private string[] stories = {"A small loan has gotten out of hand, and now \nyou must rebuild your credit and get out of debt.",
                                "If you manage your money right, you should\n get out of all this just fine.",
                                "Luckily, there is a call to arms (You’ll need your \nlegs too heh heh).",
                                "The evil BOTCORP has started ravaging \ntemples and natural settings for loot.", 
                                "Only desperate..  disposable...  noble and brave \ncitizens can stop him.",
                                "Yes just him, as the CEO of BOTCORP is the \nonly living human employed there.",
                                "We know you have some milk-flinging skills,\nso here's our deal...",
                                "You get money for every BOTCORP bot you take\ndown, then you can use it to get yourself out of debt.",
                                "To protect your family we have changed your name. \nIn this case, you will be known as Elegant Mess."};
    private float showing = 0;
    // Start is called before the first frame update
    void Start()
    {
        storyText = GetComponentsInChildren<TextMeshProUGUI>()[0];
        strikeThrough1 = GetComponentsInChildren<TextMeshProUGUI>()[1];
        strikeThrough2 = GetComponentsInChildren<TextMeshProUGUI>()[2];
        strikeThrough1.enabled = false;
        strikeThrough2.enabled = false;
        introStory = stories[storyPosition];
        Time.timeScale = 0;
    }

    // Update is called once per frame
    void Update()
    {
        showText();
        if(Input.GetMouseButtonDown(0)){
            if((int) showing <= introStory.Length){
                showing = introStory.Length - 1;
                if(storyPosition == 4){
                    delay1 = 12;
                    delay2 = 12;
                }
            } else {
                if(storyPosition < stories.Length - 1){
                    strikeThrough1.enabled = false;
                    strikeThrough2.enabled = false;
                    storyPosition++;
                    if(storyPosition == 3){
                        storyCamera.transform.position = new Vector3(40,7.7f,88);
                        Time.timeScale = 0.68f;
                    } else {
                        storyCamera.transform.position = new Vector3(-75,-1,115);
                        Time.timeScale = 0;
                    }
                    introStory = stories[storyPosition];
                    showing = 0;
                } else {
                    Time.timeScale = 1;
                    SceneManager.LoadScene("Hub");
                }
            }
        }
        if(storyPosition == 4 && (int) showing > 16){
            if(delay1 < 12){
                showing -= textSpeed;
                delay1++;
            } else {
                strikeThrough1.enabled = true;
            }
        }
        if(storyPosition == 4 && (int) showing  > 32){
            if(delay2 < 12){
                showing -= textSpeed;
                delay2++;
            } else {
                strikeThrough2.enabled = true;
            }
        }
    }

    void showText(){
        if ((int) showing <= introStory.Length){
            storyText.text = introStory.Substring(0,(int) showing);
            showing += textSpeed;
        }
    }
}
