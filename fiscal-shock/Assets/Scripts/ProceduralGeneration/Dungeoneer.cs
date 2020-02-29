using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FiscalShock.Graphs;
using ThirdParty;

/// <summary>
/// Generates a dungeon floor
/// </summary>
namespace FiscalShock.Procedural {
    public class Dungeoneer : MonoBehaviour {
        [Tooltip("Reference to player so it can be spawned somewhere in the level.")]
        public GameObject playerPrefab;

        [Tooltip("Seed for random number generator. Uses current Unix epoch time (ms) if left at 0.")]
        public long seed;

        [Tooltip("Number of vertices to generate. A higher number will generate a finer-grained mesh.")]
        public int numberOfVertices = 100;

        [Tooltip("Unit scale. All vertex coordinates are multiplied by this number.")]
        public float unitScale = 1;

        [Tooltip("Minimum x-value of a vertex.")]
        public int minX = -100;

        [Tooltip("Maximum x-value of a vertex.")]
        public int maxX = 100;

        [Tooltip("Minimum y-value of a vertex.")]
        public int minY = -100;

        [Tooltip("Maximum y-value of a vertex.")]
        public int maxY = 100;

        public Delaunay dt { get; private set; }
        public Voronoi vd { get; private set; }
        // private Delaunay masterDt;
        // private Something spanningTree;

        private MersenneTwister mt;
        private DungeonType dungeonType;

        public List<GameObject> enemies { get; } = new List<GameObject>();
        public GameObject enemyOrganizer { get; private set; }
        public GameObject thingOrganizer { get; private set; }
        public GameObject player { get; private set; }
        public GameObject organizer { get; private set; }

        public void Start() {
            initPRNG();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            generateDelaunay();
            generateVoronoi();
            sw.Stop();
            Debug.Log($"Finished generating graphs in {sw.ElapsedMilliseconds} ms");

            setDungeon();
            spawnPlayer();
        }

        public void initPRNG() {
            // Set up the PRNG
            if (seed == 0) {
                seed = System.DateTimeOffset.Now.ToUnixTimeSeconds();
            }
            // TODO Poisson disc sampling instead?
            mt = new MersenneTwister((int)seed);
            UnityEngine.Random.InitState((int)seed);
        }

        public List<double> makeRandomPoints() {
            List<double> vertices = new List<double>();
            for (int i = 0; i < numberOfVertices*2; i += 2) {
                vertices.Add(mt.Next(minX, maxX));
                vertices.Add(mt.Next(minY, maxY));
            }
            return vertices;
        }

        public void generateDelaunay() {
            dt = new Delaunay(makeRandomPoints(), minX, maxX, minY, maxY);
        }

        public void generateVoronoi() {
            vd = dt.makeVoronoi();
        }

        private void setDungeon() {
            dungeonType = GameObject.FindObjectOfType<DungeonType>();
            organizer = new GameObject();
            organizer.name = "Dungeon Parts";
            thingOrganizer = new GameObject();
            thingOrganizer.name = "Spawned Objects";
            enemyOrganizer = new GameObject();
            enemyOrganizer.name = "Enemies";
            setFloor();
            setWalls();
            randomizeCells();
            spawnEnemies();
            makeSun();  // just for fun
            makeEscapePoint();
            // make deeper point
        }

        private void makeEscapePoint() {
            int escapeSite;
            do {
                escapeSite = mt.Next(numberOfVertices-1);
            } while (isPointOnOrNearConvexHull(vd.cells[escapeSite].site));
            // Remove any currently spawned objects here
            if (vd.cells[escapeSite].spawnedObject != null) {
                Destroy(vd.cells[escapeSite].spawnedObject);
            }
            // Also remove neighbors because of clipping issues
            foreach (Cell c in vd.cells[escapeSite].neighbors) {
                if (c.spawnedObject != null) {
                    Destroy(c.spawnedObject);
                }
            }

            Vector3 where = new Vector3(vd.cells[escapeSite].site.x, dungeonType.groundTileDimensions.y, vd.cells[escapeSite].site.y);
            vd.cells[escapeSite].spawnedObject = Instantiate(dungeonType.returnPrefab, where, dungeonType.returnPrefab.transform.rotation);
        }

        private void makeSun() {
            GameObject sun = Instantiate(dungeonType.sun.prefab, dungeonType.sun.prefab.transform.position, dungeonType.sun.prefab.transform.rotation);
            sun.transform.localScale *= 10;
            sun.transform.position = new Vector3(minX - 5, dungeonType.wallTileDimensions.y * 20, minY - 5);
            sun.transform.Rotate(25, 45, -30);
            sun.name = "Sol";
        }

        private void setFloor() {
            float floorGridWidth = maxX - minX;
            float floorGridHeight = maxY - minY;

            // "Skin" the floor randomly using valid ground tiles
            int tilesPerRow = Mathf.CeilToInt(floorGridWidth / dungeonType.groundTileDimensions.x);
            int tilesPerColumn = Mathf.CeilToInt(floorGridHeight / dungeonType.groundTileDimensions.z);

            for (int i = -1; i <= tilesPerRow; ++i) {
                for (int j = -1; j <= tilesPerColumn; ++j) {
                    // Randomly select a ground tile to spawn
                    int idx = mt.Next(dungeonType.groundTiles.Count-1);
                    GameObject tileToSpawn = dungeonType.groundTiles[idx].prefab;

                    Vector3 where = new Vector3(
                        (i * dungeonType.groundTileDimensions.x) + minX,
                        0 + tileToSpawn.transform.position.y,  // ground level is 0, some tiles don't have the right origin
                        (j * dungeonType.groundTileDimensions.z) + minY
                    );
                    GameObject gro = Instantiate(tileToSpawn, where, tileToSpawn.transform.rotation);
                    gro.transform.parent = organizer.transform;

                    // Randomly rotate about the y-axis in increments of 90 deg
                    float rotation = mt.Next(4) * 90f;
                    gro.transform.Rotate(0, rotation, 0);

                    // Name it for debugging in the editor
                    gro.name = $"{tileToSpawn.name} ({i}, {j})";
                }
            }
        }

        private void setWalls() {
            // For now, just make walls along the min/max
            Vertex topLeft = new Vertex(minX * 0.9f, maxY * 0.9f);
            Vertex topRight = new Vertex(maxX * 0.9f, maxY * 0.9f);
            Vertex bottomLeft = new Vertex(minX * 0.9f, minY * 0.9f);
            Vertex bottomRight = new Vertex(maxX * 0.9f, minY * 0.9f);
            Edge top = new Edge(topLeft, topRight);
            Edge right = new Edge(topRight, bottomRight);
            Edge bottom = new Edge(bottomRight, bottomLeft);
            Edge left = new Edge(bottomLeft, topLeft);
            constructWall(top, 180, dungeonType.wallTileDimensions.x, 0);
            constructWall(right, -90, 0, -dungeonType.wallTileDimensions.x);
            constructWall(bottom, 0, -dungeonType.wallTileDimensions.x, 0);
            constructWall(left, 90, 0, dungeonType.wallTileDimensions.x);
        }

        private void constructWall(Edge wall, float wallAngle, float xDir, float yDir) {
            // TODO make walls along edges (dt convex hull)
            int wallLength = Mathf.CeilToInt((float)wall.getLength() / dungeonType.wallTileDimensions.x);
            for (int i = -1; i <= wallLength; ++i) {
                Vector3 where = new Vector3(
                    wall.p.x + (i * xDir),
                    dungeonType.groundTileDimensions.y * 0.9f,  // on top of ground tiles
                    wall.p.y + (i * yDir)
                );
                for (int j = 0; j < dungeonType.maxWallHeight; ++j) {
                    int idx = mt.Next(dungeonType.wallTiles.Count-1);
                    GameObject tileToSpawn = dungeonType.wallTiles[idx].prefab;

                    GameObject gro = Instantiate(tileToSpawn, where, tileToSpawn.transform.rotation);
                    gro.transform.parent = organizer.transform;

                    // Move higher walls up
                    gro.transform.position = new Vector3(where.x, where.y + (j * dungeonType.wallTileDimensions.y), where.z);

                    // Name it for debugging in the editor
                    gro.name = $"Wall {tileToSpawn.name} ({wallAngle}: {i}, {j})";

                    // Sometimes add a light source to the wall for now
                    // Ugly hard-coded since walls will be redone later
                    if (mt.Next(100) < 30) {
                        //int lightIdx = mt.Next(dungeonType.wallLights.Count-1);
                        GameObject lag = dungeonType.wallLights[0].prefab;
                        GameObject wallLight = Instantiate(lag, where/*dungeonType.wallLights[idx].prefab.transform.position*/, lag.transform.rotation);
                        // Attach to the wall
                        wallLight.transform.parent = gro.transform;
                    }

                    // Rotate wall and children to face interior
                    gro.transform.Rotate(0, wallAngle, 0);
                }
                if (dungeonType.wallToppers.Count > 0) {
                    int idx = mt.Next(dungeonType.wallToppers.Count-1);
                    GameObject topperToSpawn = dungeonType.wallToppers[idx].prefab;

                    GameObject top = Instantiate(topperToSpawn, where, topperToSpawn.transform.rotation);
                    top.transform.parent = organizer.transform;
                    // Rotate to face interior
                    top.transform.Rotate(0, wallAngle, 0);
                    // Move it to the top of the wall
                    top.transform.position = new Vector3(where.x, where.y + (dungeonType.maxWallHeight * dungeonType.wallTileDimensions.y), where.z);
                }
            }
        }

        private void randomizeCells() {
            foreach (Cell cell in vd.cells) {
                // Don't spawn things on the convex hull for now
                if (isPointOnOrNearConvexHull(cell.site)) {
                    continue;
                }

                // Roll 1d100 to see if we can spawn something
                int randSpawn = mt.Next(100);
                if (randSpawn > dungeonType.objectRate) {
                    // Not going to spawn something
                    continue;
                }

                // Roll another 1d100 to figure out what to spawn
                randSpawn = mt.Next(100);

                // Light sources
                float cumulativeRate = dungeonType.lightSourceRate;
                if (randSpawn < cumulativeRate) {
                    cell.spawnedObject = spawnFromList(dungeonType.lightSources, cell);
                    cell.spawnedObject.transform.parent = thingOrganizer.transform;
                    continue;
                }

                // Decorations
                cumulativeRate += dungeonType.decorationRate;
                if (randSpawn < cumulativeRate) {
                    cell.spawnedObject = spawnFromList(dungeonType.decorations, cell);
                    cell.spawnedObject.transform.parent = thingOrganizer.transform;
                    continue;
                }

                // Obstacles
                cumulativeRate += dungeonType.obstacleRate;
                if (randSpawn < cumulativeRate) {
                    cell.spawnedObject = spawnFromList(dungeonType.obstacles, cell);
                    cell.spawnedObject.transform.parent = thingOrganizer.transform;
                }
            }
        }

        private void spawnEnemies() {
            foreach (Cell cell in vd.cells) {
                // Don't spawn things on the convex hull for now
                if (isPointOnOrNearConvexHull(cell.site)) {
                    continue;
                }

                int enemySpawn = mt.Next(100);
                if (enemySpawn < dungeonType.enemyRate) {
                    GameObject enemy = spawnFromList(dungeonType.randomEnemies, cell);
                    enemies.Add(enemy);
                    enemy.transform.parent = enemyOrganizer.transform;

                    // Position enemy on top of the object already here
                    if (cell.spawnedObject != null) {
                        enemy.transform.position += new Vector3(0, cell.spawnedObject.transform.position.y, 0);
                    }

                    // Randomly resize enemy +/- the variation
                    // Example: +/- 15% => [0.85, 1.15] return values
                    float enemySize = ((mt.Next(dungeonType.enemySizeVariation * 2) - dungeonType.enemySizeVariation) / 100f) + 1;
                    enemy.transform.localScale = new Vector3(enemy.transform.localScale.x * enemySize, enemy.transform.localScale.y * enemySize, enemy.transform.localScale.z * enemySize);

                    // Attach AI here if it's not part of prefab
                }
            }
        }

        private GameObject spawnFromList(List<SpawnableObject> spawnables, Cell location) {
            // Select random index
            int idx = mt.Next(spawnables.Count-1);
            GameObject thingToSpawn = spawnables[idx].prefab;

            // Place it at the correct point
            Vector3 where = location.site.toVector3AtHeight(dungeonType.groundTileDimensions.y + thingToSpawn.transform.position.y);

            GameObject thing = Instantiate(thingToSpawn, where, thingToSpawn.transform.rotation);

            // Randomly rotate about the y-axis
            thing.transform.Rotate(0, mt.Next(360), 0);

            return thing;
        }

        private void spawnPlayer() {
            // TODO player needs to be static, do we need to do anything other than check "static" on the prefab?
            Vertex spawnPoint = dt.vertices[mt.Next(numberOfVertices-1)];
            while (isPointOnOrNearConvexHull(spawnPoint)) {
                spawnPoint = dt.vertices[mt.Next(numberOfVertices-1)];
            }
            player = Instantiate(playerPrefab, spawnPoint.toVector3AtHeight(10), playerPrefab.transform.rotation);
            player.name = "Player Character";

            // Attach any other stuff to player here
            InGameMenu menu = GameObject.FindObjectOfType<InGameMenu>();
            menu.player = player;
            menu.ChangeVolume();

            // Disable loading screen camera in this scene
            GameObject.Find("LoadCamera").GetComponent<Camera>().enabled = false;
            GameObject.Find("HUD").GetComponent<Canvas>().enabled = true;
        }

        private bool isPointOnOrNearConvexHull(Vertex point) {
            return (
                dt.convexHull.Contains(point) || point.neighborhood.Intersect(dt.convexHull).ToList().Count > 0
            );
        }
    }
}