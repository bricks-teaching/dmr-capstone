using UnityEngine;
using FiscalShock.Graphs;
using System.Collections.Generic;
using System.Linq;

namespace FiscalShock.Procedural {

    public static class Walls {
        /// <summary>
        /// All CharacterControllers should be able to fit through hallways
        /// </summary>
        private readonly static float FATTEST_CONTROLLER = 6f;

        /// <summary>
        /// Calls all functions to create walls in a dungeon
        /// </summary>
        /// <param name="d"></param>
        public static void setWalls(Dungeoneer d) {
            constructWallsOnVoronoi(d);
            constructWallsOnRooms(d);
            destroyWallsForCorridors(d);
        }

        public static void buildWalls(Dungeoneer d){
            buildWallsToKeep(d);
            constructEnemyAvoidanceBoundingBox(d);
        }

        /// <summary>
        /// Stand up trigger zones on the bounding box of the world for
        /// enemy movement AI to detect using raycasts.
        /// </summary>
        private static void constructEnemyAvoidanceBoundingBox(Dungeoneer d) {
            GameObject trigger = GameObject.Find("EnemyAvoidanceTrigger");
            GameObject floor = GameObject.FindGameObjectWithTag("Ground");
            Bounds floorBounds = floor.GetComponent<Renderer>().bounds;

            Vector3 topLeft = new Vector3(0, 0, floorBounds.extents.z*2);
            Vector3 topRight = new Vector3(floorBounds.extents.x*2, 0, floorBounds.extents.z*2);
            Vector3 bottomLeft = new Vector3(0, 0, 0);

            float xlen = Vector3.Distance(topLeft, topRight);
            float zlen = Vector3.Distance(topLeft, bottomLeft);

            Vector3 west = new Vector3(floorBounds.min.x, d.currentDungeonType.wallHeight/2, d.currentDungeonType.height/2);
            Vector3 north = new Vector3(d.currentDungeonType.width/2, d.currentDungeonType.wallHeight/2, floorBounds.max.z);
            Vector3 east = new Vector3(floorBounds.max.x, d.currentDungeonType.wallHeight/2, d.currentDungeonType.height/2);
            Vector3 south = new Vector3(d.currentDungeonType.width/2, d.currentDungeonType.wallHeight/2, floorBounds.min.z);

            setAvoidanceBoxOnSide(trigger, west, 1, d.currentDungeonType.wallHeight, zlen, d.currentDungeonType.wall.prefab);
            setAvoidanceBoxOnSide(trigger, north, xlen, d.currentDungeonType.wallHeight, 1, d.currentDungeonType.wall.prefab);
            setAvoidanceBoxOnSide(trigger, east, 1, d.currentDungeonType.wallHeight, zlen, d.currentDungeonType.wall.prefab);
            setAvoidanceBoxOnSide(trigger, south, xlen, d.currentDungeonType.wallHeight, 1, d.currentDungeonType.wall.prefab);

            // The original one isn't used, but it stays in the middle of the map, so destroy it to prevent weirdness on the AI
            UnityEngine.Object.Destroy(trigger.gameObject);
        }

        private static void setAvoidanceBoxOnSide(GameObject prefab, Vector3 position, float scaleX, float scaleY, float scaleZ, GameObject wall) {
            GameObject side = UnityEngine.Object.Instantiate(prefab, position, prefab.transform.rotation);
            side.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
            side.transform.parent = prefab.transform.parent;
            GameObject sidewall = UnityEngine.Object.Instantiate(wall, side.transform);
            sidewall.transform.localScale = new Vector3(1, 1, 1);
        }

        /// <summary>
        /// Generate walls all over the Voronoi, except for room interiors. Used
        /// to do subtractive corridor generation.
        /// </summary>
        /// <param name="d"></param>
        private static void constructWallsOnVoronoi(Dungeoneer d) {
            List<Cell> roomCells = d.roomVoronoi.SelectMany(r => r.cells).ToList();
            foreach (Cell c in d.vd.cells) {
                if (!roomCells.Contains(c)) {
                    constructWallsOnPolygon(c);
                }
            }
        }

        /// <summary>
        /// Make walls only on Voronoi rooms
        /// </summary>
        /// <param name="d"></param>
        private static void constructWallsOnRooms(Dungeoneer d) {
            foreach (VoronoiRoom r in d.roomVoronoi) {
                constructWallsOnPolygon(r.exterior);
            }
        }

        /// <summary>
        /// Make walls on all edges of a polygon
        /// </summary>
        /// <param name="d"></param>
        /// <param name="p"></param>
        private static void constructWallsOnPolygon(Polygon p) {
            foreach (Edge e in p.sides) {
                //constructWallOnEdge(d, e);
                e.isWall = true;
            }
        }

        /// <summary>
        /// Make walls along an arbitrary edge
        /// </summary>
        /// <param name="wall"></param>
        private static void constructWallOnEdge(Dungeoneer d, Edge wall) {
            // Since the prefab is stretched equally along x and y, it must be placed at the center for both x and y
            Vector3 p = wall.p.toVector3AtHeight(d.currentDungeonType.wallHeight/2);
            Vector3 q = wall.q.toVector3AtHeight(d.currentDungeonType.wallHeight/2);
            Vector3 wallCenter = (p+q)/2;

            GameObject wallObject = UnityEngine.Object.Instantiate(d.currentDungeonType.wall.prefab, wallCenter, d.currentDungeonType.wall.prefab.transform.rotation);
            wallObject.transform.parent = d.wallOrganizer.transform;
            wall.wallObjects.Add(wallObject);

            // Stretch wall
            wallObject.transform.localScale = new Vector3(
                wall.length,  // length of the original edge
                wallObject.transform.localScale.y * d.currentDungeonType.wallHeight,  // desired wall height
                wallObject.transform.localScale.z  // original prefab thickness
            );

            // Rotate the wall so that it's placed along the original edge
            Vector3 lookatme = Vector3.Cross(q - wallCenter, Vector3.up).normalized;
            wallObject.transform.LookAt(wallCenter + lookatme);

            // Attach info to game object for later use
            wallObject.GetComponent<WallInfo>().associatedEdge = wall;
            /*
            Vector3 direction = (q-p).normalized;
            #if UNITY_EDITOR
            Debug.DrawRay(p, direction * wall.length, Color.white, 512);
            #endif
            */
        }

        /// <summary>
        /// Remakes walls with a gate and corridor extending outward
        /// </summary>
        /// <param name="d"></param>
        private static void destroyWallsForCorridors(Dungeoneer d) {
            LayerMask wallMask = 1 << LayerMask.NameToLayer("Wall");
            List<Edge> wallsToKeep = new List<Edge>();
            // Need to make sure we can fit through these ones

            List<Edge> shortDestroyedWalls = new List<Edge>();
            foreach( Edge vEdge in d.vd.allEdges){
                float minDistanceToTree = 100000;
                foreach (Edge e in d.spanningTree) {
                    float slope = (e.p.y - e.q.y) / (e.p.x - e.q.x);
                    float intercept = e.p.y - (slope * e.p.x);
                    float dist = 100000;
                    float candidate0 = vEdge.p.y - ((slope * vEdge.p.x) + intercept);
                    if(Mathf.Abs(candidate0) < dist){
                        dist = Mathf.Abs(candidate0);
                    }
                    float candidate1 = vEdge.p.x - ((vEdge.p.x - slope) / intercept);
                    if(Mathf.Abs(candidate1) < dist){
                        dist = Mathf.Abs(candidate1);
                    }
                    float candidate2 = vEdge.q.x - ((vEdge.q.x - slope) / intercept);
                    if(Mathf.Abs(candidate2) < dist){
                        dist = Mathf.Abs(candidate2);
                    }
                    float candidate3 = vEdge.q.y - ((slope * vEdge.q.x) + intercept);
                    if(Mathf.Abs(candidate3) < dist){
                        dist = Mathf.Abs(candidate3);
                    }
                    if((candidate0 > 0 && candidate3 < 0) || (candidate0 < 0 && candidate3 > 0)){
                        dist = 0;
                    }
                    if(dist < minDistanceToTree){
                        minDistanceToTree = dist;
                    }
                    if(minDistanceToTree < 5){
                        break;
                    }
                }

                if(minDistanceToTree < 5){
                    removeWall(vEdge);
                }
            } 
        }

        private static void removeWall(Edge vEdge){
            vEdge.isWallToKeep = false;
            vEdge.isWall = false;
            List<Edge> incidentWalls = vEdge.p.incidentEdges.Where(e => e.isWall).ToList();
            if( incidentWalls.Count > 1){
                foreach (Edge e in incidentWalls){
                        keepWall(e);
                }
            } else if(incidentWalls.Count == 1){
                removeWall(incidentWalls[0]);
            }
            incidentWalls = vEdge.q.incidentEdges.Where(e => e.isWall).ToList();
            if( incidentWalls.Count > 1){
                foreach (Edge e in incidentWalls){
                        keepWall(e);
                }
            } else if(incidentWalls.Count == 1){
                removeWall(incidentWalls[0]);
            }
        }

        private static void keepWall(Edge vEdge){
            bool bordersOpenCell = false;
            foreach(Cell eCell in vEdge.cells){
                foreach(Edge cEdge in eCell.sides){
                    if(!cEdge.isWall){
                        bordersOpenCell = true;
                    }
                }
            }
            if(bordersOpenCell){
                vEdge.isWallToKeep = true;
                foreach (Edge e in vEdge.p.incidentEdges.Where(e => e.isWall && !e.isWallToKeep).ToList()){
                    keepWall(e);
                }
                foreach (Edge e in vEdge.q.incidentEdges.Where(e => e.isWall && !e.isWallToKeep).ToList()){
                    keepWall(e);
                }
            }
        }

        /// <summary>
        /// Remove walls that don't need to exist for performance reasons
        /// </summary>
        /// <param name="d"></param>
        /// <param name="wallsToKeep"></param>
        private static void buildWallsToKeep(Dungeoneer d) {
            foreach (VoronoiRoom r in d.roomVoronoi) {
                foreach (Edge e in r.exterior.sides) {
                    if(e.isWall){
                        e.isWallToKeep = true;
                    }
                }
            }

            closeEdges(d);
            
            //build wall objects
            foreach( Edge vEdge in d.vd.allEdges){
                if(!vEdge.isWallToKeep){
                    vEdge.isWall = false;
                } else {
                    constructWallOnEdge(d, vEdge);
                }
            }
        }

        private static void closeEdges(Dungeoneer d) {
            foreach( Cell c in d.vd.cells){
                bool edgeCell = false;
                bool openCell = false;
                foreach( Edge cEdge in c.sides){
                    if(!cEdge.isWall){
                        openCell = true;
                    }
                    if(cEdge.p.x < 0 || cEdge.p.x > 500 || cEdge.p.y < 0 || cEdge.p.y > 500 || cEdge.q.x < 0 || cEdge.q.x > 500 || cEdge.q.y < 0 || cEdge.q.y > 500){
                        edgeCell = true;
                    }
                }
                if(edgeCell && openCell){
                    foreach( Edge cEdge in c.sides){
                        cEdge.isWallToKeep = true;
                    }
                }
            }
        }
    }
}
