using System.Collections.Generic;
using UnityEngine;

namespace FiscalShock.Procedural {
    public class DungeonType : MonoBehaviour {
        [Tooltip("Dimensions of ground tiles. Prefabs should all be the same size and square on X and Z!")]
        public Vector3 groundTileDimensions;

        [Tooltip("Prefabs for all valid ground tiles.")]
        public List<SpawnableObject> groundTiles;

        [Tooltip("Prefab cube with a seamless repeating texture to use for walls. Will be stretched on x to lengthen and on y to set height. Length on z (thickness) is preserved from the prefab.")]
        public SpawnableObject wall;

        [Tooltip("Wall height.")]
        public int wallHeight;

        [Tooltip("Prefab cube with a seamless repeating texture to use for the ceiling. Will be stretched on x and y to match the dimensions of the entire ground.")]
        public SpawnableObject ceiling;

        [Tooltip("Width of corridors.")]
        public float hallWidth = 5f;

        [Tooltip("Probability of a given point being allowed to spawn any type of object.")]
        public float objectRate = 75;

        [Tooltip("Probability of a given point spawning an enemy.")]
        public float enemyRate = 15;

        [Tooltip("Percent variation of enemy size from the original prefab.")]
        public int enemySizeVariation = 15;

        [Tooltip("Prefabs for all valid randomly-spawned enemies.")]
        public List<SpawnableEnemy> randomEnemies;

        [Tooltip("Probability of a randomly-generated object being an obstacle.")]
        public float obstacleRate = 50;

        [Tooltip("Prefabs for all valid obstacles.")]
        public List<SpawnableObject> obstacles;

        [Tooltip("Probability of a randomly-generated object being a decoration.")]
        public float decorationRate = 30;

        [Tooltip("Prefabs for all valid decorations (props not meant to impede player).")]
        public List<SpawnableObject> decorations;

        [Tooltip("Temporary global light source prefab.")]
        public SpawnableObject sun;

        [Tooltip("Probability of a randomly-generated object being a light source.")]
        public float lightSourceRate = 5;

        [Tooltip("Prefabs for all valid light sources.")]
        public List<SpawnableObject> lightSources;

        [Tooltip("Prefabs for all valid light sources attached to wall.")]
        public List<SpawnableObject> wallLights;

        [Tooltip("Prefab for the object that returns you to the hub.")]
        public GameObject returnPrefab;

        [Tooltip("Prefab for the object that send you down another level in the dungeon.")]
        public GameObject delvePrefab;
    }
}
