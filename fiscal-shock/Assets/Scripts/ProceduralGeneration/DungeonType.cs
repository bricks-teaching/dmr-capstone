using System.Collections.Generic;
using UnityEngine;

namespace FiscalShock.Procedural {
    public class DungeonType : MonoBehaviour {
        /**********************************************************************/
        [Header("Graph Parameters")]

        [Tooltip("Number of vertices to generate. A higher number will generate a finer-grained mesh.")]
        public int numberOfVertices = 500;

        [Tooltip("Minimum x-value of a vertex.")]
        public int minX = -300;

        [Tooltip("Maximum x-value of a vertex.")]
        public int maxX = 300;

        [Tooltip("Minimum y-value of a vertex.")]
        public int minY = -300;

        [Tooltip("Maximum y-value of a vertex.")]
        public int maxY = 300;

        [Tooltip("Minimum number of rooms to try to generate.")]
        public int minRooms = 10;

        [Tooltip("Maximum number of rooms to try to generate.")]
        public int maxRooms = 16;

        [Tooltip("Minimum distance between any two points chosen as 'master points' (used as room centers).")]
        public double minimumDistanceBetweenMasterPoints = 32;

        [Tooltip("Percentage of edges of the master point Delaunay triangulation to add back to the spanning tree as a decimal. Adding more edges back makes more routes to rooms available.")]
        [Range(0f, 1f)]
        public float percentageOfEdgesToAddBack = 0.3f;

        [Tooltip("Indicates general size of rooms expanding outward from the master site.")]
        public int roomGrowthRadius = 2;

        /**********************************************************************/
        [Header("Enemies")]

        [Tooltip("Probability of a given point spawning an enemy.")]
        [Range(0f, 1f)]
        public float enemyRate = 0.15f;

        [Tooltip("Percent variation of enemy size from the original prefab.")]
        [Range(0f, 1f)]
        public float enemySizeVariation = 0.15f;

        [Tooltip("Prefabs for all valid randomly-spawned enemies.")]
        public List<SpawnableEnemy> randomEnemies;

        /**********************************************************************/
        [Header("Environmental Objects")]

        [Tooltip("Prefab cube with a seamless repeating texture to use for walls. Will be stretched on x and z to lengthen. Length on y (thickness) is preserved from the prefab.")]
        public SpawnableObject ground;

        [Tooltip("Prefab cube with a seamless repeating texture to use for walls. Will be stretched on x to lengthen and on y to set height. Length on z (thickness) is preserved from the prefab.")]
        public SpawnableObject wall;

        [Tooltip("Wall height.")]
        public int wallHeight;

        [Tooltip("Prefab cube with a seamless repeating texture to use for the ceiling. Will be stretched on x and y to match the dimensions of the entire ground.")]
        public SpawnableObject ceiling;

        [Tooltip("Width of corridors.")]
        public float hallWidth = 5f;

        [Tooltip("Probability of a given point being allowed to spawn any type of object.")]
        [Range(0f, 1f)]
        public float globalObjectRate = .75f;

        [Tooltip("Probability of a randomly-generated object being an obstacle.")]
        [Range(0f, 1f)]
        public float obstacleRate = .5f;

        [Tooltip("Prefabs for all valid obstacles.")]
        public List<SpawnableObject> obstacles;

        [Tooltip("Probability of a randomly-generated object being a decoration.")]
        [Range(0f, 1f)]
        public float decorationRate = 0.3f;

        [Tooltip("Prefabs for all valid decorations (props not meant to impede player).")]
        public List<SpawnableObject> decorations;

        /**********************************************************************/
        [Header("Portals and Miscellaneous")]

        [Tooltip("Temporary global light source prefab.")]
        public SpawnableObject sun;

        [Tooltip("Prefab for the object that returns you to the hub.")]
        public GameObject returnPrefab;

        [Tooltip("Prefab for the object that send you down another level in the dungeon.")]
        public GameObject delvePrefab;
    }
}
