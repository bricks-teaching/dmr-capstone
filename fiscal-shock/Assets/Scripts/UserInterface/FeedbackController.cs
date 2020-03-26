using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FeedbackController : MonoBehaviour
{
    public static Canvas HUD;
    private static TextMeshProUGUI clone;
    public TextMeshProUGUI shotLoss;
    public TextMeshProUGUI temp;
    public Image hitVinette;
    
    public void Start() {
        hitVinette.enabled = false;
    }

    public void shoot(int cost) {

        //shotLoss = GameObject.FindGameObjectWithTag("Shoot").GetComponent<TextMeshProUGUI>();
        //HUD hud = temp.GetComponent(typeof(HUD)) as HUD;
        HUD = GameObject.FindGameObjectWithTag("HUD").GetComponent<Canvas>();

        temp = shotLoss;

        //temp = HUD.GetComponent<TextMeshProUGUI>();
        //Debug.Log("temp" + temp.text);

        //TextMeshProUGUI temp = HUD.GetComponent
        //temp = temp.GetComponent<TextMeshProUGUI>();
        //temp.text = 
        

        //Debug.Log("psot " + temp.text);
        TextMeshProUGUI clone = Object.Instantiate(shotLoss);

        clone.transform.SetParent(HUD.transform);
        clone.text = "-" + (cost.ToString());
        clone.transform.localPosition = new Vector3(0,0,0);
        clone.transform.Translate(Random.Range(-135.6f, -105.0f),  Random.Range(-288.1f, -258.0f), Random.Range(-10.0f, 10.0f), Space.Self);

        Destroy(clone, 2f);

    }

    public void removeHit() {
        hitVinette.enabled = false;
    }

}
