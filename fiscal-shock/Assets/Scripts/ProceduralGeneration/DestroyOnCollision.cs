using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCollision : MonoBehaviour {
    public List<string> tagsToAvoid = new List<string> { "Ground" };
    public bool markedForDestruction = false;
    private int ticks = 0;
    private Rigidbody rb;

    private void Start() {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    public void OnTriggerEnter(Collider col) {
        GameObject other = col.gameObject;
        DestroyOnCollision otherScript = other.GetComponent<DestroyOnCollision>();
        if (otherScript == null) {
            return;
        }
        if (tagsToAvoid.Contains(other.tag) && !otherScript.markedForDestruction) {
            markedForDestruction = true;
            Debug.Log($"{gameObject.name}: Destroying myself!");
        }
    }

    private void FixedUpdate() {
        ticks++;
        if (markedForDestruction) {
            Destroy(gameObject);
        }
        // disable script after a while so it doesn't eat up cpu
        if (ticks > 16) {
            Destroy(rb);
            this.enabled = false;
        }
    }
}
