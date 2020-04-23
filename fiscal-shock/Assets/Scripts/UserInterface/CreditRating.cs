using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreditRating : MonoBehaviour {
    public Image abysmal;
    public Image poor;
    public Image fair;
    public Image good;
    public Image excellent;
    public TextMeshProUGUI texto;

    public void Awake() {
        StateManager.creditBarScript = this;
        Invoke("updateRatingBar", 1f);
    }

    /// <summary>
    /// Refresh the credit rating bar on the pause menu.
    /// </summary>
    public void updateRatingBar() {
        gameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 0, 1);
        gameObject.SetActive(true);

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
        gameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        gameObject.SetActive(false);
    }
}
