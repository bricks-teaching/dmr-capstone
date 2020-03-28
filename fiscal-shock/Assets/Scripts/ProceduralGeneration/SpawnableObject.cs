using UnityEngine;

[System.Serializable]
public class SpawnableObject {
    [Tooltip("Object prefab.")]
    public GameObject prefab;

    [Tooltip("Weight on [0, 1] applied against spawn chance. Higher weights indicate more likely spawning.")]
    [Range(0f, 1f)]
    public float weight = 1;

    [Tooltip("Apply a random color to the first mesh renderer encountered in the game object. The prefab should have the colorable mesh renderer at the top.")]
    public bool randomizeColor;
    public bool alsoColorLight;
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
