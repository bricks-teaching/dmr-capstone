using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CreditRating : MonoBehaviour {
    public Image abysmal;
    public Image poor;
    public Image fair;
    public Image good;
    public Image excellent;
    public TextMeshProUGUI texto;
    public TextMeshProUGUI delta;

    public void Awake() {
        Invoke("updateRatingBar", 1f);
    }

    public void OnEnable() {
        StateManager.creditBarScript = this;
    }

    /// <summary>
    /// Refresh the credit rating bar on the pause menu.
    /// </summary>
    public void updateRatingBar() {
        //gameObject.SetActive(true);
        //gameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 0, 1);

        excellent.fillAmount = Mathf.Clamp01((StateManager.creditScore - StateManager.ExcellentCredit.min) / StateManager.ExcellentCredit.range);
        good.fillAmount = Mathf.Clamp01((StateManager.creditScore - StateManager.GoodCredit.min) / StateManager.GoodCredit.range);
        fair.fillAmount = Mathf.Clamp01((StateManager.creditScore - StateManager.FairCredit.min) / StateManager.FairCredit.range);
        poor.fillAmount = Mathf.Clamp01((StateManager.creditScore - StateManager.PoorCredit.min) / StateManager.PoorCredit.range);
        abysmal.fillAmount = Mathf.Clamp01((StateManager.creditScore - StateManager.AbysmalCredit.min) / StateManager.AbysmalCredit.range);

        // C# can't compare structs...
        if (StateManager.currentRating.rating == StateManager.ExcellentCredit.rating) {
            texto.color = excellent.color;
        } else if (StateManager.currentRating.rating == StateManager.GoodCredit.rating) {
            texto.color = good.color;
        } else if (StateManager.currentRating.rating == StateManager.FairCredit.rating) {
            texto.color = fair.color;
        } else if (StateManager.currentRating.rating == StateManager.PoorCredit.rating) {
            texto.color = poor.color;
        } else if (StateManager.currentRating.rating == StateManager.AbysmalCredit.rating) {
            texto.color = abysmal.color;
        }
        texto.text = StateManager.currentRating.rating;
        //gameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        int deltint = Mathf.RoundToInt(StateManager.creditScore - StateManager.lastCreditScore);
        delta.text = $"{(deltint > 0? "+" : "")}{deltint}";
        if (StateManager.lastCreditScore < StateManager.AbysmalCredit.min) {
            delta.text = "";
        }
        if (deltint > 0) {
            delta.color = excellent.color;
        } else if (deltint < 0) {
            delta.color = abysmal.color;
        } else {
            delta.color = Color.white;
        }
        //gameObject.SetActive(false);
        StartCoroutine(showCreditDelta(deltint));
    }

    public IEnumerator showCreditDelta(int deltint) {
        if (StateManager.income.Count > 0) {
            TextMeshProUGUI pt = GameObject.FindGameObjectWithTag("Player Text").GetComponent<TextMeshProUGUI>();
            if (deltint > 0) {
                pt.color = Color.green;
            } else if (deltint < 0) {
                pt.color = Color.red;
            } else {
                pt.color = Color.white;
            }
            pt.text = $"Credit score change:\n{(deltint > 0? "+" : "")}{deltint}";
            for (float i = 2f; i >= 0; i -= Time.deltaTime) {
                pt.color = new Color(pt.color.r, pt.color.g, pt.color.b, i/2f);
                yield return null;
            }
            pt.color = new Color(pt.color.r, pt.color.g, pt.color.b, 0);
        }
        yield return null;
    }
}
