using UnityEngine;
using FiscalShock.Graphs;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FiscalShock.Procedural {
    public static class Walls {
        /// <summary>
        /// Calls all functions to create walls in a dungeon
        /// </summary>
        /// <param name="d"></param>
        public static void setWalls(Dungeoneer d) {
            constructWallsOnVoronoi(d);
            constructWallsOnRooms(d);
            List<GameObject> wallsToKeep = destroyWallsForCorridors(d);
            destroyLagWalls(d, wallsToKeep);
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

            Vector3 topLeft = new Vector3(floorBounds.min.x, 0, floorBounds.max.z);
            Vector3 topRight = new Vector3(floorBounds.max.x, 0, floorBounds.max.z);
            Vector3 bottomLeft = new Vector3(floorBounds.min.x, 0, floorBounds.min.z);

            float xlen = Vector3.Distance(topLeft, topRight);
            float zlen = Vector3.Distance(topLeft, bottomLeft);

            Vector3 west = new Vector3(floorBounds.min.x, d.currentDungeonType.wallHeight/2, 0);
            Vector3 north = new Vector3(0, d.currentDungeonType.wallHeight/2, floorBounds.max.z);
            Vector3 east = new Vector3(floorBounds.max.x, d.currentDungeonType.wallHeight/2, 0);
            Vector3 south = new Vector3(0, d.currentDungeonType.wallHeight/2, floorBounds.min.z);

            setAvoidanceBoxOnSide(trigger, west, 1, d.currentDungeonType.wallHeight, zlen);
            setAvoidanceBoxOnSide(trigger, north, xlen, d.currentDungeonType.wallHeight, 1);
            setAvoidanceBoxOnSide(trigger, east, 1, d.currentDungeonType.wallHeight, zlen);
            setAvoidanceBoxOnSide(trigger, south, xlen, d.currentDungeonType.wallHeight, 1);

            // The original one isn't used, but it stays in the middle of the map, so destroy it to prevent weirdness on the AI
            UnityEngine.Object.Destroy(trigger.gameObject);
        }

        private static void setAvoidanceBoxOnSide(GameObject prefab, Vector3 position, float scaleX, float scaleY, float scaleZ) {
            GameObject side = UnityEngine.Object.Instantiate(prefab, position, prefab.transform.rotation);
            side.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
            side.transform.parent = prefab.transform.parent;
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
                    constructWallsOnPolygon(d, c);
                }
            }
        }

        /// <summary>
        /// Make walls only on Voronoi rooms
        /// </summary>
        /// <param name="d"></param>
        private static void constructWallsOnRooms(Dungeoneer d) {
            foreach (VoronoiRoom r in d.roomVoronoi) {
                constructWallsOnPolygon(d, r.exterior);
            }
        }

        /// <summary>
        /// Make walls on all edges of a polygon
        /// </summary>
        /// <param name="d"></param>
        /// <param name="p"></param>
        private static void constructWallsOnPolygon(Dungeoneer d, Polygon p) {
            foreach (Edge e in p.sides) {
                constructWallOnEdge(d, e);
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
            wall.isWall = true;

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
        private static List<GameObject> destroyWallsForCorridors(Dungeoneer d) {
            LayerMask wallMask = 1 << LayerMask.NameToLayer("Wall");
            List<GameObject> wallsToKeep = new List<GameObject>();

            foreach (Edge e in d.spanningTree) {
                float len = e.length;

                Vector3 p = e.p.toVector3AtHeight(1);
                Vector3 q = e.q.toVector3AtHeight(1);
                Vector3 direction = (q-p).normalized;
                // uncomment to debug raycasts
                /*
                #if UNITY_EDITOR
                Debug.DrawRay(p, direction * len, Color.blue, 512);
                #endif
                */

                // Cast a ray to destroy initial set of walls. SphereCast does
                // not always catch all walls close to the sphere center
                List<RaycastHit> hits = new List<RaycastHit>(Physics.CapsuleCastAll(p, p + Vector3.up * d.currentDungeonType.wallHeight, d.currentDungeonType.hallWidth, direction, len, wallMask));
                foreach (RaycastHit hit in hits) {
                    hit.collider.gameObject.GetComponent<WallInfo>().associatedEdge.isWall = false;
                    hit.collider.gameObject.name = $"Destroyed {hit.collider.gameObject.name}";
                    UnityEngine.Object.Destroy(hit.collider.gameObject);
                }

                // Cast a wider sphere to determine what walls to keep during cleanup
                foreach (RaycastHit hit in Physics.SphereCastAll(p, d.currentDungeonType.hallWidth * 7, direction, len, wallMask)) {
                    wallsToKeep.Add(hit.collider.gameObject);
                }
            }

            return wallsToKeep;
        }

        /// <summary>
        /// Remove walls that don't need to exist for performance reasons
        /// </summary>
        /// <param name="d"></param>
        /// <param name="wallsToKeep"></param>
        private static void destroyLagWalls(Dungeoneer d, List<GameObject> wallsToKeep) {
            foreach (VoronoiRoom r in d.roomVoronoi) {
                foreach (Edge e in r.exterior.sides) {
                    wallsToKeep.AddRange(e.wallObjects);
                }
            }

            foreach (GameObject w in GameObject.FindGameObjectsWithTag("Wall")) {
                if (!wallsToKeep.Contains(w)) {
                    w.GetComponent<WallInfo>().associatedEdge.isWall = false;
                    w.name = $"Destroyed {w.name}";
                    UnityEngine.Object.Destroy(w);
                }
            }
        }
    }
}
