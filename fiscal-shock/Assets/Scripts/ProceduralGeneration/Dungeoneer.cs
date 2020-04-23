using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FiscalShock.Graphs;
using ThirdParty;
using UnityEngine.Rendering;
using FiscalShock.GUI;

/// <summary>
/// Generates a dungeon floor
/// </summary>
namespace FiscalShock.Procedural {
    public class Dungeoneer : MonoBehaviour {
        [Tooltip("Reference to player prefab so it can be spawned somewhere in the level.")]
        public GameObject playerPrefab;

        [Tooltip("Seed for random number generator. Uses current Unix epoch time (ms) if left at 0.")]
        public long seed;

        [Tooltip("Available dungeon themes.")]
        public List<DungeonTypeData> dungeonThemes;

        [Header("Development/Debug")]
        [Tooltip("List of weapons to start with when you load the game from this scene directly.")]
        public List<GameObject> debugWeapons;

        [Tooltip("Cash on hand to start at.")]
        public float debugCash;
        [Tooltip("Set the state manager value")]
        public int timesEntered;
        [Tooltip("Set the state manager value")]
        public int currentFloor;

        /* Variables set during runtime */
        public DungeonType currentDungeonType { get; set; }
        public MersenneTwister mt { get; private set; }

        /* Graphs */
        public Delaunay dt { get; private set; }
        public Voronoi vd { get; private set; }
        public Delaunay masterDt { get; private set; }
        public List<Edge> spanningTree { get; private set; }
        public List<VoronoiRoom> roomVoronoi { get; private set; }
        public List<Cell> validCells { get; private set; }

        /* Scene organization */
        public List<GameObject> enemies { get; } = new List<GameObject>();
        public GameObject player { get; private set; }
        public GameObject organizer { get; private set; }
        public GameObject wallOrganizer { get; private set; }
        public GameObject enemyOrganizer { get; private set; }
        public GameObject thingOrganizer { get; private set; }

        public void Start() {
            Settings.loadSettings();
            Debug.Log($"Starting to load");
            initPRNG();

            // Set theme based on state manager selection
            // Currently takes the first matching the enum, could
            // have different "configurations" of the same theme
            // and pick randomly later
            currentDungeonType = dungeonThemes
                .Where(d => d.typeEnum == StateManager.selectedDungeon)
                //.Where(d => d.typeEnum == DungeonTypeEnum.Mine)  // uncomment to go straight to mines when testing dungeon scene
                .Select(d => d.gameObject)
                .First()
                .GetComponent<DungeonType>();

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
            setLighting();
            sw.Stop();
            Debug.Log($"Finished spawning stuff in {sw.ElapsedMilliseconds} ms");
        }

        public void initPRNG() {
            // Set up the PRNG
            if (seed == 0) {
                seed = System.DateTimeOffset.Now.ToUnixTimeSeconds();
            }
            mt = new MersenneTwister((int)seed);
            UnityEngine.Random.InitState((int)seed);
            Debug.Log($"Using seed {seed}");
        }

        public void generateDelaunay() {
            Debug.Log("Generating Delaunay");
            Poisson dist = new Poisson(currentDungeonType.minimumPoissonDistance, currentDungeonType.width, currentDungeonType.height);
            dt = new Delaunay(dist.vertices);
            Debug.Log($"Generated Delaunay with {dt.vertices.Count} vertices.");
        }

        public void generateVoronoi() {
            Debug.Log("Generating Voronoi");
            vd = dt.makeVoronoi();
            Debug.Log($"Generated Voronoi with {vd.vertices.Count} vertices.");
        }

        public void generateRoomGraphs() {
            Debug.Log("Generating room graphs");
            // pick how many rooms to make
            int rooms = mt.Next(currentDungeonType.minRooms, currentDungeonType.maxRooms);
            List<Vertex> masterDelaunayPoints = new List<Vertex>();

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
                    if (d < currentDungeonType.minimumDistanceBetweenMasterPoints) {
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
                masterDelaunayPoints.Add(dt.vertices[selection]);
                infinityGuard = 0;
            }

            // get triangulation of those points
            masterDt = new Delaunay(masterDelaunayPoints);

            // get spanning tree
            //spanningTree = masterDt.edges.Distinct().ToList();
            spanningTree = masterDt.findSpanningTreeBFS();

            // add back some edges from triangulation to provide multiple routes
            int edgesToAddBack = Mathf.CeilToInt((masterDt.edges.Count - spanningTree.Count) * currentDungeonType.percentageOfEdgesToAddBack);
            for (int i = 0; i < edgesToAddBack; ++i) {
                // randomly pick an edge
                int t;
                Edge e;
                do {
                    t = mt.Next(0, masterDt.edges.Count-1);
                    e = masterDt.edges[t];
                } while (spanningTree.Contains(e));
                spanningTree.Add(e);
            }

            // do voronoi blending around points using the original voronoi cells
            // does not merge separate rooms!
            roomVoronoi = masterDelaunayPoints.Select(v => new VoronoiRoom(v)).Where(r => r.site != null).ToList();
            for (int i = 0; i < currentDungeonType.roomGrowthRadius; ++i) {
                foreach (VoronoiRoom r in roomVoronoi) {
                    r.grow();
                }
            }

            validCells = getValidCells();
        }

        private void setDungeon() {
            organizer = new GameObject();
            organizer.name = "Dungeon Parts";
            wallOrganizer = new GameObject();
            wallOrganizer.name = "Walls";
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
            if (currentDungeonType.spanningTreeTrack.prefab != null) {
                Edgewise.generateOnEdges(this, spanningTree, currentDungeonType.spanningTreeTrack.prefab);
            }
            randomizeCells();
            sw.Stop();
            Debug.Log($"Generating objects took {sw.ElapsedMilliseconds} ms");
            sw.Reset();

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
        /// Set up per-dungeon lighting parameters.
        /// </summary>
        private void setLighting() {
            GameObject.FindGameObjectWithTag("Dungeon Directional Light").SetActive(currentDungeonType.enableDirectionalLight);
            RenderSettings.fogColor = currentDungeonType.fogColor;
            RenderSettings.ambientMode = currentDungeonType.ambientMode;
            switch (RenderSettings.ambientMode) {
                case AmbientMode.Flat:
                    RenderSettings.ambientLight = currentDungeonType.ambientColor;
                    break;

                case AmbientMode.Trilight:
                    RenderSettings.ambientSkyColor = currentDungeonType.skyColor;
                    RenderSettings.ambientEquatorColor = currentDungeonType.equatorColor;
                    RenderSettings.ambientGroundColor = currentDungeonType.groundColor;
                    break;

                // Skybox has no additional settings at this time, but it would
                // include a reference to the skybox material if changed
                default:
                    break;
            }

            Light flashlight = GameObject.FindGameObjectWithTag("Player Flashlight").GetComponent<Light>();
            flashlight.enabled = true;
            flashlight.intensity = currentDungeonType.playerFlashlightIntensity;
            flashlight.range = currentDungeonType.playerFlashlightRange;
            flashlight.spotAngle = currentDungeonType.playerFlashlightRadius;
        }

        /// <summary>
        /// Don't spawn things that weren't in rooms
        /// </summary>
        /// <returns></returns>
        private List<Cell> getValidCells() {
            return vd.cells.Where(c => c.room != null).ToList();
        }

        /// <summary>
        /// Randomize and spawn environmental objects.
        /// </summary>
        private void randomizeCells() {
            Debug.Log("Randomizing and spawning environmental objects");
            foreach (Cell cell in vd.cells) {
            //foreach (Cell cell in validCells) {
                // Don't spawn things on the convex hull for now
                if (isPointOnOrNearConvexHull(cell.site) || cell.sides.All(e => e.isWall)) {
                    continue;
                }

                // Roll 1d100 to see if we can spawn something
                float randSpawn = mt.NextFloat();
                if (randSpawn > currentDungeonType.globalObjectRate) {
                    // Not going to spawn something
                    continue;
                }

                // Roll another 1d100 to figure out what to spawn
                randSpawn = mt.NextFloat();

                // Decorations
                float cumulativeRate = currentDungeonType.decorationRate;
                if (randSpawn < cumulativeRate) {
                    cell.spawnedObject = spawnFromList(currentDungeonType.decorations, cell);
                    cell.spawnedObject.transform.parent = thingOrganizer.transform;
                    continue;
                }

                // Obstacles
                cumulativeRate += currentDungeonType.obstacleRate;
                if (randSpawn < cumulativeRate) {
                    cell.spawnedObject = spawnFromList(currentDungeonType.obstacles, cell);
                    cell.spawnedObject.transform.parent = thingOrganizer.transform;
                }
            }
        }

        /// <summary>
        /// Randomize and spawn enemies.
        /// </summary>
        private void spawnEnemies() {
            if (StateManager.startedFromDungeon) {
                Debug.Log("Adjusting state manager");
                StateManager.timesEntered = timesEntered;
                StateManager.currentFloor = currentFloor;
            }
            Debug.Log("Spawning enemies");
            // Global flatter mod based on times entered
            float globalModifier = Mathf.Clamp(Mathf.Log(StateManager.timesEntered+1, 3), 0.5f, Mathf.Infinity);

            // Scale point value to reward going deeper. Close to double by B3F, 3x by B5F, etc. Scale enemy damage with respect to this!
            float pointValueModifier = ((Mathf.Log(StateManager.currentFloor+1, 1.15f) * (StateManager.currentFloor+1)/32) + 1) * globalModifier;

            // Flatter health mod
            float healthModifier = Mathf.Log(StateManager.currentFloor+1, 2.7f) * globalModifier;

            // Damage mod
            float damageModifier = ((Mathf.Log(StateManager.currentFloor+1, 2) * StateManager.currentFloor/12) + 0.5f) * globalModifier;

            // Not strongly recommended to alter speed. Enemies could eventually get ridiculously fast, and then it's not very fun. Would also imply we should increase animation speeds, so that would track more stuff. Must be very careful and definitely use an asymptotic function to prevent ludicrous speed

            // Accuracy could be improved, but I'm not worrying about it right now

            Debug.Log($"Enemy stat modifiers: global {globalModifier}, value {pointValueModifier}, health {healthModifier}, damage {damageModifier}");
            foreach (Cell cell in vd.cells) {
                // Don't spawn things on the convex hull for now
                if (isPointOnOrNearConvexHull(cell.site) || cell.sides.All(e => e.isWall)) {
                    continue;
                }

                float enemySpawn = mt.NextFloat();
                if (enemySpawn < currentDungeonType.enemyRate) {
                    GameObject enemy = spawnEnemy(currentDungeonType.randomEnemies, cell);
                    enemies.Add(enemy);
                    enemy.transform.parent = enemyOrganizer.transform;

                    // Position enemy on top of the object already here
                    if (cell.spawnedObject != null) {
                        enemy.transform.position += new Vector3(0, cell.spawnedObject.transform.position.y, 0);
                    }

                    // Randomly resize enemy +/- the variation
                    // Example: +/- 15% => [0.85, 1.15] return values
                    // * 100 * 2 to double the interval (negative/positive) and then make it an int; MersenneTwister can only do a range on ints
                    float enemySize = ((mt.Next(Mathf.CeilToInt(currentDungeonType.enemySizeVariation * 100) * 2) - currentDungeonType.enemySizeVariation) / 100f) + 1;
                    enemy.transform.localScale = new Vector3(enemy.transform.localScale.x * enemySize, enemy.transform.localScale.y * enemySize, enemy.transform.localScale.z * enemySize);

                    // Adjust enemy stats based on SpawnableEnemy setup
                    EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
                    EnemyShoot es = enemy.GetComponent<EnemyShoot>();
                    eh.pointValue *= pointValueModifier;
                    eh.startingHealth *= healthModifier;
                    es.attackDamage *= damageModifier;
                }
            }
        }

        private GameObject spawnEnemy(List<SpawnableEnemy> spawnables, Cell location) {
            return spawnFromList(spawnables.Cast<SpawnableObject>().ToList(), location);
        }

        private GameObject spawnFromList(List<SpawnableObject> spawnables, Cell location) {
            // Select random index
            int idx;
            float chance;

            // Check if this should be spawned based on weight
            do {
                idx = mt.Next(spawnables.Count-1);
                chance = mt.NextFloat();
            } while (spawnables[idx].weight < chance);

            GameObject thingToSpawn = spawnables[idx].prefab;

            // Place it at the correct point
            Vector3 where = location.site.toVector3AtHeight(thingToSpawn.transform.position.y);

            GameObject thing = Instantiate(thingToSpawn, where, thingToSpawn.transform.rotation);
            thing.name = $"{thingToSpawn.name} @ {location.site.id}";

            // Randomly rotate about the y-axis
            thing.transform.Rotate(0, mt.Next(360), 0);

            // Randomize color
            // Can only select the first material, so not all things may be recolored.
            if (spawnables[idx].randomizeColor) {
                float r = mt.NextFloat();
                float g = mt.NextFloat();
                float b = mt.NextFloat();
                Material mat = thing.GetComponentInChildren<Renderer>().material;
                mat?.SetColor("_Color", new Color(r, g, b, 1f));
                if (spawnables[idx].alsoColorLight) {
                    Light lite = thing.GetComponentInChildren<Light>();
                    if (lite != null) {
                        lite.color = new Color(r, g, b);
                    }
                }
            }

            return thing;
        }

        private void spawnPlayer() {
            Debug.Log("Spawning player");

            Vertex spawnPoint;
            do {  // Don't spawn the player on portals. Warning: infinite loop if there are only 1-2 cells
                spawnPoint = masterDt.vertices[mt.Next(masterDt.vertices.Count-1)];
            } while (spawnPoint.cell.hasPortal);
            SpawnPoint spawner = GameObject.FindGameObjectWithTag("Spawn Point").GetComponent<SpawnPoint>();
            spawner.transform.position = spawnPoint.toVector3AtHeight(currentDungeonType.wallHeight * 0.8f);
            player = spawner.spawnPlayer();

            // Attach any other stuff to player here
            Cheats cheater = GameObject.FindObjectOfType<Cheats>();
            cheater.player = player;
            cheater.playerMovement = player.GetComponentInChildren<PlayerMovement>();
            InGameMenu menu = GameObject.FindObjectOfType<InGameMenu>();
            menu.player = player;

            // Set up HUD
            GameObject hud = GameObject.Find("HUD");
            hud.GetComponent<Canvas>().enabled = true;
            HUD hudScript = hud.GetComponentInChildren<HUD>();
            hudScript.escapeHatch = GameObject.Find("Escape Point").transform;
            hudScript.playerTransform = player.transform;

            // Enable firing script (disabled in hub)
            PlayerShoot shootScript = player.GetComponentInChildren<PlayerShoot>();
            shootScript.enabled = true;

            if (StateManager.startedFromDungeon) {
                // Give player weapons when starting in dungeon, since that implies it's a dev starting in the editor
                Debug.Log("Looks like you launched this scene directly from the editor. Here's some free stuff...");
                StateManager.cashOnHand = debugCash;
                Inventory playerInventory = GameObject.FindGameObjectWithTag("Player Inventory").GetComponentInChildren<Inventory>();
                foreach (GameObject weapon in debugWeapons) {
                    GameObject realWeapon = Instantiate(weapon);
                    playerInventory.addWeapon(realWeapon);
                }
            }

            // Enable temporary player invincibility on spawn
            StartCoroutine(player.GetComponentInChildren<PlayerHealth>().enableIframes(5f));
        }

        private bool isPointOnOrNearConvexHull(Vertex point) {
            return
                dt.convexHull.Contains(point) || point.neighborhood.Intersect(dt.convexHull).ToList().Count > 0
            ;
        }
    }
}
