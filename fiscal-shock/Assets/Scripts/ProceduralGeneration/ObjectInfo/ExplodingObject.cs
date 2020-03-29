using UnityEngine;

public class ExplodingObject : MonoBehaviour {
    public AudioClip explosionNoise;
    public GameObject explosionEffect;
    private AudioSource aso;
    public float explosionRadius = 6f;
    public float explosionDamage = 30f;
    private int PLAYER;
    private int ENEMY;
    private int hitMask;
    private Renderer[] meshes;
    private bool exploding;

    private void Start() {
        aso = gameObject.AddComponent<AudioSource>();
        PLAYER = LayerMask.NameToLayer("Player");
        ENEMY = LayerMask.NameToLayer("Enemy");
        hitMask = (1 << PLAYER) | (1 << ENEMY);
        meshes = GetComponentsInChildren<Renderer>();
    }

    private void OnTriggerEnter(Collider col) {
        if (exploding) {
            return;
        }
        switch (col.gameObject.tag) {
            case "Bullet":
            case "Missile":
                exploding = true;
                // Damage things that were hit
                Collider[] hitObjects = Physics.OverlapSphere(transform.position, explosionRadius, hitMask);
                foreach (Collider hit in hitObjects) {
                    float randomizedDamage = explosionDamage * Random.Range(0.5f, 2f);
                    int layerHit = hit.gameObject.layer;
                    if (layerHit == PLAYER) {
                        hit.gameObject.GetComponentInChildren<PlayerHealth>().takeDamage(randomizedDamage);
                    }
                    if (layerHit == ENEMY) {
                        EnemyHealth eh = hit.gameObject.GetComponentInChildren<EnemyHealth>();
                        eh.takeDamage(randomizedDamage);
                        eh.showDamageExplosion(null, 0f);
                        eh.stun(3f);
                    }
                }

                // Hide game object, can't destroy it just yet
                foreach (Renderer r in meshes) {
                    r.enabled = false;
                }

                Destroy(Instantiate(explosionEffect, transform.position, transform.rotation), 2f);

                // Show a light
                GameObject light = new GameObject();
                light.transform.position = transform.position;
                Light l = light.AddComponent<Light>();
                l.color = new Color(1f, 0.29f, 0, 0.5f);
                l.intensity = 5f;
                l.range = explosionRadius * 2;
                l.type = LightType.Point;
                Destroy(light, 1f);

                // Play sound
                aso.PlayOneShot(explosionNoise, 3 * Settings.volume);
                Destroy(gameObject, 2f);
                break;

            default:
                break;
        }
    }
}
