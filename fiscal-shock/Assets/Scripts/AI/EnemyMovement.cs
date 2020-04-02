using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using FiscalShock.Procedural;

public class EnemyMovement : MonoBehaviour {
    [Tooltip("The speed at which the object moves.")]
    public float movementSpeed = 3f;

    [Tooltip("The speed at which the object turns.")]
    public float rotationSpeed = 7f;

    [Tooltip("The absolute minimum distance away from the player.")]
    public float safeRadiusMin = 4f;

    [Tooltip("Creates safe radius in case object ends up too close to player.")]
    public float safeRadiusMax = 5f;

    [Tooltip("How close the player needs to be before being pursued.")]
    public float visionRadius = 35f;

    public GameObject player;

    public bool stunned { get; set; }

    private float safeRadiusAvg;
    private float destinationRefreshDistance;
    private float distanceFromPlayer;
    private bool destinationReached = true;
    private Vector2 destination;
    private Vector2 prevPlayerFlatPos;
    private Rigidbody enemyRb;
    private EnemyShoot shootScript;
    public AnimationManager animationManager;

    void Start() {
        enemyRb = GetComponent<Rigidbody>();
        shootScript = GetComponent<EnemyShoot>();
        //enemyRb.useGravity = false;
        //enemyRb.isKinematic = true;
        safeRadiusAvg = (safeRadiusMax + safeRadiusMin) / 2;
        destinationRefreshDistance = safeRadiusAvg - safeRadiusMin;

        if (player == null) {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        prevPlayerFlatPos = new Vector2(player.transform.position.x, player.transform.position.z);
    }

    void FixedUpdate() {
        if (player == null || (Vector3.Distance(player.transform.position, gameObject.transform.position) > visionRadius) || stunned) {
            // TODO drunkard's walk
            animationManager.playIdleAnimation();
            shootScript.spottedPlayer = false;
            return;
        }
        shootScript.spottedPlayer = true;

        // Don't interrupt other animations to play movement
        if (!animationManager.animator.isPlaying) {
            animationManager.playMoveAnimation();
        }

        // This is the only variable that really needs to be a R3 vector - to look in the correct direction.
        Vector3 playerDirection = (player.transform.position - transform.position).normalized;
        Vector2 flatPosition = new Vector2(transform.position.x, transform.position.z);
        Vector2 playerFlatPosition = new Vector2(player.transform.position.x, player.transform.position.z);

        if (gameObject.tag == "Lobber") {
            playerDirection.y = 0;
        }

        Quaternion rotationToPlayer = Quaternion.LookRotation(playerDirection);

        // Need 2D distance - will only consider how far away enemy is from player on x,z plane.
        distanceFromPlayer = Vector2.Distance(playerFlatPosition, flatPosition);

        // TODO: Some of this movement might still be a little buggy, but it's less likely the enemy will sink to the ground now =|
        enemyRb.MoveRotation(Quaternion.Slerp(gameObject.transform.rotation, rotationToPlayer, Time.fixedDeltaTime * rotationSpeed));

        if (distanceFromPlayer > safeRadiusMax) {
            enemyRb.MovePosition(transform.position + (playerDirection * movementSpeed * Time.fixedDeltaTime));
        }

        if (distanceFromPlayer < safeRadiusMin) {
            enemyRb.MovePosition(transform.position - (playerDirection * movementSpeed * Time.fixedDeltaTime));
        }

       if (distanceFromPlayer <= safeRadiusMax && distanceFromPlayer >= safeRadiusMin) {
            // If the enemy hasn't reached its destination and the player hasn't moved out of the safe radius destination reach
            if (!destinationReached && Vector2.Distance(playerFlatPosition, prevPlayerFlatPos) < destinationRefreshDistance) {
                // The next position the player would move to in a straight line to the destination.
                Vector2 linearTarget = flatPosition + ((destination - flatPosition).normalized * movementSpeed * Time.fixedDeltaTime);

                // The difference between the distance between the safe radius average distance and the distance
                // between the player and the next linear target position.
                float distanceDiff = safeRadiusAvg - Vector2.Distance(playerFlatPosition, linearTarget);

                // Get the actual next position
                Vector2 targetPosition = getOrbitalCoordinate(linearTarget, playerFlatPosition, distanceDiff);

                if (Vector2.Distance(targetPosition, destination) < destinationRefreshDistance) {
                    destinationReached = true;
                }

                enemyRb.MovePosition(new Vector3(targetPosition.x, transform.position.y, targetPosition.y));
            }

            else if (!destinationReached) {
                Vector2 coordinate = getRandomCircularCoordinate();
                destination = (coordinate * safeRadiusAvg) + playerFlatPosition;
                prevPlayerFlatPos = playerFlatPosition;
            }

            else {
                Vector2 coordinate = getRandomCircularCoordinate();
                destination = (coordinate * safeRadiusAvg) + playerFlatPosition;
                destinationReached = false;
            }
        }
    }

    private Vector2 getRandomCircularCoordinate() {
        float angle = Random.Range(0.0f, 2 * Mathf.PI);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    private Vector2 getOrbitalCoordinate(Vector2 linearPosition, Vector2 playerPosition, float distance) {
        Vector2 result = playerPosition - linearPosition;
        float xValue = result.x / Mathf.Sqrt(Mathf.Pow(result.x, 2) + Mathf.Pow(result.y, 2));
        float yValue = result.y / Mathf.Sqrt(Mathf.Pow(result.x, 2) + Mathf.Pow(result.y, 2));
        result.x = xValue;
        result.y = yValue;
        return linearPosition - (distance * result);
    }

    public float getDistanceFromPlayer() {
        return distanceFromPlayer;
    }
}
