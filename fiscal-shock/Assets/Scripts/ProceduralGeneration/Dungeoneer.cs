using System.Collections.Generic;
using UnityEngine;
using FiscalShock.Graphs;
using ThirdParty;

/// <summary>
/// Generates graphs and stuff
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

        private GameObject player;

        private MersenneTwister mt;
        private DungeonType dungeonType;

        public void Start() {
            initPRNG();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            generateDelaunay();
            generateVoronoi();
            sw.Stop();
            Debug.Log($"Finished generating graphs in {sw.ElapsedMilliseconds} ms");

            setDungeon();
            // TODO point default camera at a "loading" screen
            // Once the player is spawned, make that the active camera
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
            setFloor();
            setWalls();
            // assign values to each dt point if not on hull OR neighbor of hull
            // spawn obstacles
            // spawn decos
            // spawn enemies
        }

        private void setFloor() {
            float floorGridWidth = maxX - minX;
            float floorGridHeight = maxY - minY;

            // TODO remove later
            // Widen the default floor to catch the player
            GameObject floor = GameObject.Find("DefaultGround");
            floor.transform.localScale = new Vector3(floorGridWidth, 1, floorGridHeight);

            // "Skin" the floor randomly using valid ground tiles
            int tilesPerRow = Mathf.CeilToInt(floorGridWidth / dungeonType.groundTileDimensions.x);
            int tilesPerColumn = Mathf.CeilToInt(floorGridHeight / dungeonType.groundTileDimensions.z);

            for (int i = 0; i < tilesPerRow; ++i) {
                for (int j = 0; j < tilesPerColumn; ++j) {
                    // Randomly select a ground tile to spawn
                    int idx = mt.Next(dungeonType.groundTiles.Count-1);
                    GameObject tileToSpawn = dungeonType.groundTiles[idx].prefab;

                    Vector3 where = new Vector3(
                        (i * dungeonType.groundTileDimensions.x) + minX,
                        0 + tileToSpawn.transform.position.y,  // ground level is 0
                        (j * dungeonType.groundTileDimensions.z) + minY
                    );
                    GameObject gro = Instantiate(tileToSpawn, where, tileToSpawn.transform.rotation);

                    // Randomly rotate about the y-axis in increments of 90 deg
                    float rotation = mt.Next(4) * 90f;
                    gro.transform.Rotate(0, rotation, 0);

                    // Scale to a square of a specified size
                    gro.transform.localScale = new Vector3(dungeonType.unitScale, dungeonType.unitScale, dungeonType.unitScale);

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
            constructWall(top, 180, dungeonType.wallTileDimensions.x, 0);  //
            constructWall(right, -90, 0, -dungeonType.wallTileDimensions.x);
            constructWall(bottom, 0, -dungeonType.wallTileDimensions.x, 0); //
            constructWall(left, 90, 0, dungeonType.wallTileDimensions.x);
        }

        private void constructWall(Edge wall, float wallAngle, float xDir, float yDir) {
            int wallLength = Mathf.CeilToInt((float)wall.getLength() / dungeonType.wallTileDimensions.x);
            for (int i = 0; i < wallLength; ++i) {
                Vector3 where = new Vector3(
                    wall.p.x + (i * xDir),
                    dungeonType.groundTileDimensions.y * 0.9f,  // on top of ground tiles
                    wall.p.y + (i * yDir)
                );
                for (int j = 0; j < dungeonType.maxWallHeight; ++j) {
                    int idx = mt.Next(dungeonType.wallTiles.Count-1);
                    GameObject tileToSpawn = dungeonType.wallTiles[idx].prefab;

                    GameObject gro = Instantiate(tileToSpawn, where, tileToSpawn.transform.rotation);

                    // Rotate wall to face interior
                    gro.transform.Rotate(0, wallAngle, 0);
                    // Move higher walls up
                    gro.transform.position = new Vector3(where.x, where.y + j * dungeonType.wallTileDimensions.y, where.z);

                    // Scale to a square of a specified size
                    gro.transform.localScale = new Vector3(dungeonType.unitScale, dungeonType.unitScale, dungeonType.unitScale);

                    // Name it for debugging in the editor
                    gro.name = $"Wall {tileToSpawn.name} ({wallAngle}: {i}, {j})";
                }
                if (dungeonType.wallToppers.Count > 0) {
                    int idx = mt.Next(dungeonType.wallToppers.Count-1);
                    GameObject topperToSpawn = dungeonType.wallToppers[idx].prefab;

                    GameObject top = Instantiate(topperToSpawn, where, topperToSpawn.transform.rotation);
                    // Rotate to face interior
                    top.transform.Rotate(0, wallAngle, 0);
                    // Move it to the top of the wall
                    top.transform.position = new Vector3(where.x, where.y + dungeonType.maxWallHeight * dungeonType.wallTileDimensions.y, where.z);
                }
            }
        }

        public void spawnPlayer() {
            // TODO player needs to be static
            player = Instantiate(playerPrefab, dt.vertices[mt.Next(numberOfVertices-1)].toVector3AtHeight(10), playerPrefab.transform.rotation);
            player.name = "Player Character";

            // DEBUG adding the graph display
            GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            FiscalShock.Demo.ProceduralMeshRenderer meshScript = mainCamera.AddComponent<FiscalShock.Demo.ProceduralMeshRenderer>();
            meshScript.dungen = this;
            meshScript.delaunayColor = new Color(1, 0, 1);
            meshScript.voronoiColor = new Color(0.5f, 0.5f, 1);
            meshScript.delaunayRenderHeight = 5f;
            meshScript.voronoiRenderHeight = 7f;
        }
    }
}