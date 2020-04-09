using UnityEngine;

public class ExplodingObject : MonoBehaviour {
    public AudioClip explosionNoise;
    public GameObject explosionEffect;
    private AudioSource aso;
    public float explosionRadius = 6f;
    public float explosionDamage = 30f;
    private int PLAYER;
    private int ENEMY;
    private int EXPLOSIVE;
    private int hitMask;
    private Renderer[] meshes;
    private Collider[] colliders;
    private bool exploding;
    private PlayerHealth player;

    private void Start() {
        aso = gameObject.AddComponent<AudioSource>();
        PLAYER = LayerMask.NameToLayer("Player");
        ENEMY = LayerMask.NameToLayer("Enemy");
        EXPLOSIVE = LayerMask.NameToLayer("Explosive");
        hitMask = (1 << PLAYER) | (1 << ENEMY) | (1 << EXPLOSIVE);
        meshes = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerHealth>();
    }

    private void OnTriggerEnter(Collider col) {
        if (exploding) {
            return;
        }
        switch (col.gameObject.tag) {
            case "Enemy Projectile":
            case "Bullet":
            case "Missile":
                explode();
                break;

            default:
                break;
        }
    }

    public void explode() {
        exploding = true;
        // Damage things that were hit
        Collider[] hitObjects = Physics.OverlapSphere(transform.position, explosionRadius, hitMask);
        foreach (Collider hit in hitObjects) {
            if (hit.gameObject == gameObject) {
                continue;
            }
            float randomizedDamage = explosionDamage * Random.Range(0.5f, 2f);
            int layerHit = hit.gameObject.layer;
            if (layerHit == PLAYER) {
                player.takeDamage(randomizedDamage);
            }
            if (layerHit == ENEMY) {
                EnemyHealth eh = hit.gameObject.GetComponentInChildren<EnemyHealth>();
                if (eh != null) {
                    hit.attachedRigidbody.isKinematic = false;
                    hit.attachedRigidbody.AddExplosionForce(randomizedDamage*10, transform.position + Vector3.up, explosionRadius, 3f);
                    eh?.takeDamage(randomizedDamage);
                    eh?.showDamageExplosion(null, 0f);
                    eh?.stun(3f);
                }
            }
            if (layerHit == EXPLOSIVE) {
                ExplodingObject boom = hit.gameObject.GetComponentInChildren<ExplodingObject>();
                if (boom == null) {
                    boom = hit.gameObject.GetComponentInParent<ExplodingObject>();
                }
                if (boom != null && !boom.exploding) {
                    boom.explode();
                }
            }
        }

        // Hide game object, can't destroy it just yet
        foreach (Renderer r in meshes) {
            r.enabled = false;
        }
        foreach (Collider c in colliders) {
            c.enabled = false;
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
        aso.PlayOneShot(explosionNoise, Settings.volume);
        Destroy(gameObject, 2f);
    }
}
