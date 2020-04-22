using UnityEngine;

public class TargetBehavior : MonoBehaviour {
    private int rising = 23;
    private int falling = 23;
    public IntroStory story;
    private float speed;
    private bool wasHit = false;
    private int hitCount = 0;
    private bool moving = false;
    private bool movingLeft = false;

    /// <summary>
    /// Target will move back and forth until it is hit
    /// </summary>
    void Update() {
        if (rising < 23) {
            transform.position += new Vector3(0, 0.1f, 0);
            rising++;
            if (rising == 23) {
                if (Random.value > .5) {
                    movingLeft = true;
                }
                moving = true;
                speed = Random.value * 0.05f + .04f;
            }
        }
        if (falling < 23) {
            transform.position += new Vector3(0, -0.1f, 0);
            falling++;
            if (falling == 23 && hitCount < 2) {
                rising = 0;
                wasHit = false;
            }
        }
        if (moving) {
            if (transform.position.z < 105) {
                movingLeft = false;
            }
            else if (transform.position.z > 113) {
                movingLeft = true;
            }
            transform.position += new Vector3(0, 0, (movingLeft ? -speed : speed));
        }
    }

    void OnCollisionEnter(Collision col) {
        if (moving && !wasHit) {
            wasHit = true;
            hitCount++;
            moving = false;
            story.hitTarget();
            falling = 0;
        }
    }

    public void activateTarget() {
        rising = 0;
        wasHit = false;
        hitCount = 0;
    }
}
