using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour {
    public float lifetime = 0.9f;

    public IEnumerator timeout() {
        yield return new WaitForSeconds(lifetime);
        gameObject.SetActive(false);
        yield return null;
    }
}
