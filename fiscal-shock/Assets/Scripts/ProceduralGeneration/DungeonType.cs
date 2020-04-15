using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FiscalShock.Procedural {
    public class DungeonType : MonoBehaviour {
        /**********************************************************************/
        [Header("Graph Parameters")]

        [Tooltip("Minimum distance between any two points. The maximum number of points is found by floor(height / floor(minimumPoissonDistance/Poisson.dimensions)) * floor(width / floor(minimumPoissonDistance/Poisson.dimensions)).")]
        public float minimumPoissonDistance = 4f;

        [Tooltip("Width (2D x) of the dungeon floor.")]
        public int width = 200;

        [Tooltip("Height/depth (2D y) of the dungeon floor.")]
        public int height = 200;

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

        [Tooltip("Optional object to spawn along the spanning tree. If empty, nothing is spawned.")]
        public SpawnableObject spanningTreeTrack;

        /**********************************************************************/
        [Header("Lighting")]

        [Tooltip("Enable the global directional light for more ambient lighting.")]
        public bool enableDirectionalLight = true;

        [Tooltip("Strength of the player's flashlight.")]
        public float playerFlashlightIntensity = 0.5f;

        [Tooltip("Cone radius of the player's flashlight. Should be adjusted if player FOV is adjusted.")]
        [Range(0f, 179f)]
        public float playerFlashlightRadius = 110f;

        [Tooltip("Range of the player's flashlight.")]
        public float playerFlashlightRange = 300f;

        [Tooltip("Ambient lighting mode. Trilight is equal to Gradient in the inspector, and Flat is equal to Color.")]
        public AmbientMode ambientMode = AmbientMode.Trilight;

        [Tooltip("Sky color, only used with Trilight lighting.")]
        [ColorUsageAttribute(true,true)]
        public Color skyColor = new Color(128, 128, 128);

        [Tooltip("Equator color, only used with Trilight lighting.")]
        [ColorUsageAttribute(true,true)]
        public Color equatorColor = new Color(116, 128, 136);

        [Tooltip("Ground color, only used with Trilight lighting.")]
        [ColorUsageAttribute(true,true)]
        public Color groundColor = new Color(48, 44, 36);

        [Tooltip("Ambient color, only used with Flat lighting.")]
        [ColorUsageAttribute(true,true)]
        public Color ambientColor = new Color(255, 255, 255);

        [Tooltip("Fog color. Fog is always enabled due to the far clip plane of the camera.")]
        public Color fogColor = new Color(0, 0, 0);

        /**********************************************************************/
        [Header("Portals")]

        [Tooltip("Prefab for the object that returns you to the hub.")]
        public GameObject returnPrefab;

        [Tooltip("Prefab for the object that send you down another level in the dungeon.")]
        public GameObject delvePrefab;
    }

    [System.Serializable]
    public class DungeonTypeData {
        [Tooltip("Game object for a dungeon type.")]
        public GameObject gameObject;

        [Tooltip("Enum value for a dungeon type. Should be a one-to-one mapping for each dungeon type to each enum value! Used to pass info to/from state manager.")]
        public DungeonTypeEnum typeEnum;
    }
}
