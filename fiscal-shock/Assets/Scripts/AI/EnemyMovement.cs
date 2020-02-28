using UnityEngine;

// TODO: The script is outputting the player's curret 

public class EnemyMovement : MonoBehaviour {
    [Tooltip("The speed at which the object moves.")]
    public float movementSpeed = 5f;

    [Tooltip("The speed at which the object turns.")]
    public float rotationSpeed = 7f;

    [Tooltip("The absolute minimum distance away from the player.")]
    public float safeRadiusMin = 4f;

    [Tooltip("Creates safe radius in case object ends up too close to player.")]
    public float safeRadiusMax = 5f;

    private float safeRadiusAvg;
    private float distanceFromPlayer;
    private bool destinationReached = true;
    private Vector3 destination;
    private Rigidbody enemyRb;
    private GameObject player;
    private readonly float destinationRefreshDistance = 0.25f;

    // Start is called before the first frame update
    void Start() {
        player = GameObject.Find("Player");
        enemyRb = GetComponent<Rigidbody>();
        enemyRb.useGravity = false;
        enemyRb.isKinematic = true;
        safeRadiusAvg = (safeRadiusMax + safeRadiusMin) / 2;
    }

    void FixedUpdate() {
        if (player == null) {
            return;
        }

        Vector3 playerDirection = (player.transform.position - transform.position).normalized;

        if (gameObject.tag == "Lobber") {
            playerDirection.y = 0;
        }

        Quaternion rotationToPlayer = Quaternion.LookRotation(playerDirection);
        distanceFromPlayer = Vector3.Distance(player.transform.position, transform.position);

        // TODO: This makes the enemy lean back when they start inside of the unsafe radius
        enemyRb.MoveRotation(Quaternion.Slerp(gameObject.transform.rotation, rotationToPlayer, Time.fixedDeltaTime * rotationSpeed));

        if (!destinationReached) {
            if (distanceFromPlayer > safeRadiusMax) {
                enemyRb.MovePosition(transform.position + (playerDirection * movementSpeed * Time.fixedDeltaTime));
            }

            if (distanceFromPlayer < safeRadiusMin) {
                enemyRb.MovePosition(transform.position - (playerDirection * movementSpeed * Time.fixedDeltaTime));
            }

            if (distanceFromPlayer <= safeRadiusMax  && distanceFromPlayer >= safeRadiusMin) { // The object is within the safe radius.
                // Calculation to move around the player on the next point to the destination
                // 1) Calculate the next position
                Vector3 linearPosition = transform.position + ((destination - transform.position).normalized * movementSpeed * Time.fixedDeltaTime);
                Vector2 twoDimLinearPos = new Vector2(linearPosition.x, linearPosition.z);
                Vector2 twoDimPlayerPos = new Vector2(player.transform.position.x, player.transform.position.z);

                // 2) Calculate the difference between safeRadiusAvg and the distance between the player and object
                float distanceDiff = safeRadiusAvg - Vector2.Distance(twoDimPlayerPos, twoDimLinearPos);

                // 3) Get the vector with the distance applied.
                Vector3 targetPosition = getOrbitalCoordinate(twoDimLinearPos, distanceDiff);

                if (Vector3.Distance(targetPosition, destination) < destinationRefreshDistance) {
                    destinationReached = true;
                }

                enemyRb.MovePosition(targetPosition);
            }
        }
        else {
            Vector2 coordinate = getRandomCircularCoordinate();
            Vector2 twoDimDestination = (coordinate * (safeRadiusMin + safeRadiusMax) / 2) + new Vector2(player.transform.position.x,player.transform.position.z);
            destination = new Vector3(twoDimDestination.x, transform.position.y, twoDimDestination.y);
            destinationReached = false;
        }
    }

    private Vector2 getRandomCircularCoordinate() {
        float angle = Random.Range(0.0f, 2 * Mathf.PI);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    private Vector3 getOrbitalCoordinate(Vector2 linearPosition, float distance) {
        Vector2 playerPosition = new Vector2(player.transform.position.x, player.transform.position.z);
        Vector2 result = playerPosition - linearPosition;
        float xValue = result.x/Mathf.Sqrt(Mathf.Pow(result.x, 2) + Mathf.Pow(result.y, 2));
        float yValue = result.y/Mathf.Sqrt(Mathf.Pow(result.x, 2) + Mathf.Pow(result.y, 2));
        result.x = xValue;
        result.y = yValue;
        result = linearPosition - (distance * result);
        return new Vector3(result.x, transform.position.y, result.y);
    }

    public float getDistanceFromPlayer() {
        return distanceFromPlayer;
    }
}
