using System.Collections.Generic;
using UnityEngine;

public class DungeonType : MonoBehaviour {
    [Tooltip("Target Unity unit size to scale models to.")]
    public float unitScale = 1;

    [Tooltip("Prefabs for all valid randomly-spawned enemies.")]
    public List<GameObject> randomEnemies;

    [Tooltip("Dimensions of ground tiles. Prefabs should all be the same size and square on X and Z!")]
    public Vector3 groundTileDimensions;

    [Tooltip("Prefabs for all valid ground tiles.")]
    public List<SpawnableObject> groundTiles;

    [Tooltip("Dimensions of wall tiles. Prefabs should all be the same size and square on X and Y!")]
    public Vector3 wallTileDimensions;

    [Tooltip("Number of wall tiles to stack to make wall.")]
    public int maxWallHeight;

    [Tooltip("Prefabs for all valid wall tiles.")]
    public List<SpawnableObject> wallTiles;

    [Tooltip("Optional list of prefabs that decorate the top of the wall. Prefabs are assumed to be prescaled and appropriately rotated.")]
    public List<SpawnableObject> wallToppers;

    [Tooltip("Prefabs for all valid obstacles (props with collision).")]
    public List<SpawnableObject> obstacles;

    [Tooltip("Prefabs for all valid decorations (props without collision).")]
    public List<SpawnableObject> decorations;
}
