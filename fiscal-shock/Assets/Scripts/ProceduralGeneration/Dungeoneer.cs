using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FiscalShock.Graphs;
using ThirdParty;
using UnityEngine.AI;

/// <summary>
/// Generates a dungeon floor
/// </summary>
namespace FiscalShock.Procedural {
    public class Dungeoneer : MonoBehaviour {
        [Tooltip("Reference to player so it can be spawned somewhere in the level.")]
        public GameObject playerPrefab;

        [Tooltip("Seed for random number generator. Uses current Unix epoch time (ms) if left at 0.")]
        public long seed; //1584410049

        [Tooltip("Number of vertices to generate. A higher number will generate a finer-grained mesh.")]
        public int numberOfVertices = 100;

        /*
        [Tooltip("Unit scale. All vertex coordinates are multiplied by this number.")]
        public float unitScale = 1;
        */

        [Tooltip("Minimum x-value of a vertex.")]
        public int minX = -100;

        [Tooltip("Maximum x-value of a vertex.")]
        public int maxX = 100;

        [Tooltip("Minimum y-value of a vertex.")]
        public int minY = -100;

        [Tooltip("Maximum y-value of a vertex.")]
        public int maxY = 100;

        [Tooltip("Minimum number of rooms to try to generate.")]
        public int minRooms = 10;

        [Tooltip("Maximum number of rooms to try to generate.")]
        public int maxRooms = 16;

        [Tooltip("Minimum distance between any two points chosen as 'master points' (used as room centers).")]
        public double minimumDistanceBetweenMasterPoints = 32;

        [Tooltip("Percentage of edges of the master point Delaunay triangulation to add back to the spanning tree as a decimal. Adding more edges back makes more routes to rooms available.")]
        public float percentageOfEdgesToAddBack = 0.3f;

        [Tooltip("Indicates general size of rooms expanding outward from the master site.")]
        public int roomGrowthRadius = 3;

        public Delaunay dt { get; private set; }
        public Voronoi vd { get; private set; }
        public Delaunay masterDt { get; private set; }
        public List<Edge> spanningTree { get; private set; }
        public List<VoronoiRoom> roomVoronoi { get; private set; }
        public List<Cell> validCells { get; private set; }

        public MersenneTwister mt { get; private set; }
        public DungeonType dungeonType { get; set; }

        public List<GameObject> enemies { get; } = new List<GameObject>();
        public GameObject player { get; private set; }
        public GameObject organizer { get; private set; }
        public GameObject wallOrganizer { get; private set; }
        public GameObject enemyOrganizer { get; private set; }
        public GameObject thingOrganizer { get; private set; }

        public void Start() {
            Debug.Log($"Starting to load");
            initPRNG();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            generateDelaunay();
            generateVoronoi();
            generateRoomGraphs();
            sw.Stop();
            Debug.Log($"Finished generating graphs in {sw.ElapsedMilliseconds} ms");

            sw.Reset();
            sw.Start();
            Debug.Log("Starting object generation");
            setDungeon();
            spawnPlayer();
            sw.Stop();
            Debug.Log($"Finished spawning stuff in {sw.ElapsedMilliseconds} ms");
        }

        public void initPRNG() {
            // Set up the PRNG
            if (seed == 0) {
                seed = System.DateTimeOffset.Now.ToUnixTimeSeconds();
            }
            // TODO Poisson disc sampling instead?
            mt = new MersenneTwister((int)seed);
            UnityEngine.Random.InitState((int)seed);
            Debug.Log($"Using seed {seed}");
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
            Debug.Log("Generating Delaunay");
            dt = new Delaunay(makeRandomPoints());
        }

        public void generateVoronoi() {
            Debug.Log("Generating Voronoi");
            vd = dt.makeVoronoi();
        }

        public void generateRoomGraphs() {
            Debug.Log("Generating room graphs");
            // pick how many rooms to make
            int rooms = mt.Next(minRooms, maxRooms);
            List<Vertex> masterDelaunayPoints = new List<Vertex>();
            List<double> masterDelaunayPointsFlat = new List<double>();

            // warning: potential infinite loops!
            int infinityGuard = 0;
            for (int i = 0; i < rooms; ++i) {
                int selection = mt.Next(0, dt.vertices.Count-1);
                bool tooClose = false;

                // don't pick points on convex hull, they are naughty
                if (isPointOnOrNearConvexHull(dt.vertices[selection]) || dt.vertices[selection].cell.getArea() > VoronoiRoom.MAX_CELL_AREA) {
                    infinityGuard++;
                    i--;
                    continue;
                }

                // don't pick something too close to another point already chosen
                foreach (Vertex v in masterDelaunayPoints) {
                    double d = dt.vertices[selection].getDistanceTo(v);
                    if (d < minimumDistanceBetweenMasterPoints) {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose && infinityGuard < dt.vertices.Count) {
                    i--;
                    infinityGuard++;
                    continue;
                }

                // if we get this far, it's okay to add
                masterDelaunayPointsFlat.Add(dt.vertices[selection].x);
                masterDelaunayPointsFlat.Add(dt.vertices[selection].y);
                masterDelaunayPoints.Add(dt.vertices[selection]);
                infinityGuard = 0;
            }

            // get triangulation of those points
            masterDt = new Delaunay(masterDelaunayPointsFlat);

            // get spanning tree
            //spanningTree = masterDt.edges.Distinct().ToList();
            spanningTree = masterDt.findSpanningTreeBFS();

            // add back some edges from triangulation to provide multiple routes
            int edgesToAddBack = Mathf.CeilToInt(masterDt.edges.Count * percentageOfEdgesToAddBack);
            for (int i = 0; i < edgesToAddBack; ++i) {
                // randomly pick an edge
                int t;
                Edge e;
                do {
                    t = mt.Next(0, masterDt.edges.Count-1);
                    e = masterDt.edges[t];
                } while (spanningTree.Contains(e));
            }

            // do voronoi blending around points using the original voronoi cells
            // does not merge separate rooms!
            roomVoronoi = masterDelaunayPoints.Select(v => new VoronoiRoom(v)).Where(r => r.site != null).ToList();
            for (int i = 0; i < roomGrowthRadius; ++i) {
                foreach (VoronoiRoom r in roomVoronoi) {
                    r.grow();
                }
            }

            validCells = getValidCells();
        }

        private void setDungeon() {
            dungeonType = GameObject.FindObjectOfType<DungeonType>();
            organizer = new GameObject();
            organizer.name = "Dungeon Parts";
            wallOrganizer = new GameObject();
            wallOrganizer.name = "Wall Tiles";
            wallOrganizer.transform.parent = organizer.transform;
            thingOrganizer = new GameObject();
            thingOrganizer.name = "Spawned Objects";
            thingOrganizer.transform.parent = organizer.transform;
            enemyOrganizer = new GameObject();
            enemyOrganizer.name = "Enemies";

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            Debug.Log("Starting floor generation");
            sw.Start();
            Floor.setFloor(this);
            sw.Stop();
            Debug.Log($"Generating floors took {sw.ElapsedMilliseconds} ms");
            sw.Reset();

            Debug.Log("Starting wall generation");
            sw.Start();
            Walls.setWalls(this);
            sw.Stop();
            Debug.Log($"Generating walls took {sw.ElapsedMilliseconds} ms");
            sw.Reset();

            Debug.Log("Starting object placement");
            sw.Start();
            randomizeCells();
            sw.Stop();
            Debug.Log($"Generating objects took {sw.ElapsedMilliseconds} ms");
            sw.Reset();

            makeSun();  // just for fun
            Debug.Log("Starting portal placement");
            sw.Start();
            Portals.makeDelvePoint(this);
            Portals.makeEscapePoint(this);
            sw.Stop();
            Debug.Log($"Placing portals took {sw.ElapsedMilliseconds} ms");
            sw.Reset();

            Debug.Log("Starting enemy placement");
            sw.Start();
            spawnEnemies();
            sw.Stop();
            Debug.Log($"Placing enemies took {sw.ElapsedMilliseconds} ms");
            sw.Reset();
        }

        /// <summary>
        /// Don't spawn things that weren't in rooms
        /// </summary>
        /// <returns></returns>
        private List<Cell> getValidCells() {
            return vd.cells.Where(c => c.room != null).ToList();
        }

        private void makeSun() {
            GameObject sun = Instantiate(dungeonType.sun.prefab, dungeonType.sun.prefab.transform.position, dungeonType.sun.prefab.transform.rotation);
            sun.transform.localScale *= 10;
            sun.transform.position = new Vector3(minX - 5, dungeonType.wallHeight * 4, minY - 5);
            sun.transform.Rotate(25, 45, -30);
            sun.name = "Sol";
        }

        private void randomizeCells() {
            Debug.Log("Randomizing and spawning environmental objects");
            foreach (Cell cell in validCells) {
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
            Debug.Log("Spawning enemies");
            foreach (Cell cell in validCells) {
                // Don't spawn things on the convex hull for now
                if (isPointOnOrNearConvexHull(cell.site)) {
                    continue;
                }

                int enemySpawn = mt.Next(100);
                if (enemySpawn < dungeonType.enemyRate) {
                    GameObject enemy = spawnEnemy(dungeonType.randomEnemies, cell);
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

                    // Adjust enemy stats based on SpawnableEnemy setup

                }
            }
        }

        private GameObject spawnEnemy(List<SpawnableEnemy> spawnables, Cell location) {
            return spawnFromList(spawnables.Cast<SpawnableObject>().ToList(), location);
        }

        private GameObject spawnFromList(List<SpawnableObject> spawnables, Cell location) {
            // Select random index
            int idx = mt.Next(spawnables.Count-1);
            GameObject thingToSpawn = spawnables[idx].prefab;

            // Place it at the correct point
            Vector3 where = location.site.toVector3AtHeight(thingToSpawn.transform.position.y);

            GameObject thing = Instantiate(thingToSpawn, where, thingToSpawn.transform.rotation);
            thing.name = $"{thingToSpawn.name} @ {location.site.id}";

            // Randomly rotate about the y-axis
            thing.transform.Rotate(0, mt.Next(360), 0);

            return thing;
        }

        private void spawnPlayer() {
            Debug.Log("Spawning player");
            Vertex spawnPoint = masterDt.vertices[mt.Next(masterDt.vertices.Count-1)];
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) {
                player = Instantiate(playerPrefab, playerPrefab.transform.position, playerPrefab.transform.rotation);
            }
            CharacterController playerController = player.GetComponentInChildren<CharacterController>();
            playerController.enabled = false;
            player.transform.position = spawnPoint.toVector3AtHeight(dungeonType.wallHeight * 0.8f);
            playerController.enabled = true;

            // Attach any other stuff to player here
            Cheats cheater = GameObject.FindObjectOfType<Cheats>();
            cheater.player = player;
            cheater.playerController = player.GetComponentInChildren<CharacterController>();
            InGameMenu menu = GameObject.FindObjectOfType<InGameMenu>();
            menu.player = player;

            // Disable loading screen camera in this scene
            GameObject.Find("LoadCamera").GetComponent<Camera>().enabled = false;
            GameObject hud = GameObject.Find("HUD");
            hud.GetComponent<Canvas>().enabled = true;
            HUD hudScript = hud.GetComponentInChildren<HUD>();
            hudScript.escapeHatch = GameObject.Find("Escape Point").transform;
            hudScript.playerTransform = player.transform;

            // Enable firing script and spotlight (disabled in hub)
            PlayerShoot shootScript = player.GetComponentInChildren<PlayerShoot>();
            shootScript.enabled = true;
            shootScript.Start();
            player.GetComponentInChildren<Light>().enabled = true;
        }

        private bool isPointOnOrNearConvexHull(Vertex point) {
            return
                dt.convexHull.Contains(point) || point.neighborhood.Intersect(dt.convexHull).ToList().Count > 0
            ;
        }
    }
}
