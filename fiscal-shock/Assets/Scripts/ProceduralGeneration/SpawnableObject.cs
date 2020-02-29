using UnityEngine;

[System.Serializable]
public class SpawnableObject {
    public GameObject prefab;
}

[System.Serializable]
public class SpawnableEnemy : SpawnableObject {
    [Tooltip("Base health value.")]
    public float baseHealth = 30f;

    [Tooltip("Base damage value.")]
    public float baseDamage = 3f;

    [Tooltip("The speed at which the object moves.")]
    public float movementSpeed = 3f;

    [Tooltip("The speed at which the object turns.")]
    public float rotationSpeed = 7f;

    [Tooltip("The absolute minimum distance away from the player.")]
    public float safeRadiusMin = 4f;

    [Tooltip("Creates safe radius in case object ends up too close to player.")]
    public float safeRadiusMax = 5f;
}
