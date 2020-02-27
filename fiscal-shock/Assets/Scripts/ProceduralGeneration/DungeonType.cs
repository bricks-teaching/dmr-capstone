using System.Collections.Generic;
using UnityEngine;

namespace FiscalShock.Procedural {
    public class DungeonType : MonoBehaviour {
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

        [Tooltip("Optional list of prefabs that decorate the top of the wall.")]
        public List<SpawnableObject> wallToppers;

        [Tooltip("Probability of a given point being allowed to spawn any type of object.")]
        public float objectRate = 75;

        [Tooltip("Probability of a given point spawning an enemy.")]
        public float enemyRate = 15;

        [Tooltip("Percent variation of enemy size from the original prefab.")]
        public int enemySizeVariation = 15;

        [Tooltip("Prefabs for all valid randomly-spawned enemies.")]
        public List<SpawnableObject> randomEnemies;

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
    }
}
