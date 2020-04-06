﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FeedbackController : MonoBehaviour
{
    private Queue<TextMeshProUGUI> shotLosses { get; } = new Queue<TextMeshProUGUI>();
    private int numLossesToDisplay = 12;
    private Queue<TextMeshProUGUI> earns { get; } = new Queue<TextMeshProUGUI>();
    private int numEarnsToDisplay = 12;
    public TextMeshProUGUI shotLoss;
    public TextMeshProUGUI earn;
    public Image hitVignette;

    public void Start() {
        for (int i = 0; i < numLossesToDisplay; ++i) {
            TextMeshProUGUI sh = Instantiate(shotLoss);
            sh.transform.SetParent(transform);
            sh.enabled = false;
            shotLosses.Enqueue(sh);
        }
        for (int i = 0; i < numEarnsToDisplay; ++i) {
            TextMeshProUGUI ea = Instantiate(earn);
            ea.transform.SetParent(transform);
            ea.enabled = false;
            earns.Enqueue(ea);
        }
    }

    public void shoot(int cost) {
        TextMeshProUGUI clone = shotLosses.Dequeue();
        clone.text = "-" + (cost.ToString());
        clone.transform.localPosition = new Vector3(0,0,0);
        clone.transform.Translate(Random.Range(-10.6f, 10.0f),  Random.Range(-10.1f, 10.0f), Random.Range(-10.0f, 10.0f), Space.Self);
        clone.enabled = true;
        shotLosses.Enqueue(clone);

        StartCoroutine(timeout(clone, 2f));
    }

    public void profit(float amount) {
        TextMeshProUGUI clone = earns.Dequeue();
        clone.text = "+" + (amount.ToString());
        clone.transform.localPosition = new Vector3(0,0,0);
        clone.transform.Translate(Random.Range(160.6f, 170.0f),  Random.Range(-10.1f, 10.0f), Random.Range(-10.0f, 10.0f), Space.Self);
        clone.enabled = true;
        earns.Enqueue(clone);

        StartCoroutine(timeout(clone, 2f));
    }

    private IEnumerator timeout(TextMeshProUGUI text, float duration) {
        yield return new WaitForSeconds(duration);
        text.enabled = false;
        yield return null;
    }
}