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

            Vector3 west = new Vector3(floorBounds.min.x, d.dungeonType.wallHeight/2, 0);
            Vector3 north = new Vector3(0, d.dungeonType.wallHeight/2, floorBounds.max.z);
            Vector3 east = new Vector3(floorBounds.max.x, d.dungeonType.wallHeight/2, 0);
            Vector3 south = new Vector3(0, d.dungeonType.wallHeight/2, floorBounds.min.z);

            trigger.transform.position = west;
            trigger.transform.localScale = new Vector3(0, d.dungeonType.wallHeight, zlen);

            GameObject no = UnityEngine.Object.Instantiate(trigger, north, trigger.transform.rotation);
            no.transform.localScale = new Vector3(xlen, d.dungeonType.wallHeight, 1);
            no.transform.parent = trigger.transform.parent;

            GameObject ea = UnityEngine.Object.Instantiate(trigger, east, trigger.transform.rotation);
            ea.transform.localScale = new Vector3(1, d.dungeonType.wallHeight, zlen);
            ea.transform.parent = trigger.transform.parent;

            GameObject so = UnityEngine.Object.Instantiate(trigger, south, trigger.transform.rotation);
            so.transform.localScale = new Vector3(xlen, d.dungeonType.wallHeight, 1);
            so.transform.parent = trigger.transform.parent;
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
            Vector3 p = wall.p.toVector3AtHeight(d.dungeonType.wallHeight/2);
            Vector3 q = wall.q.toVector3AtHeight(d.dungeonType.wallHeight/2);
            Vector3 wallCenter = (p+q)/2;

            GameObject wallObject = UnityEngine.Object.Instantiate(d.dungeonType.wall.prefab, wallCenter, d.dungeonType.wall.prefab.transform.rotation);
            wallObject.transform.parent = d.wallOrganizer.transform;
            wall.wallObjects.Add(wallObject);

            // Stretch wall
            wallObject.transform.localScale = new Vector3(
                wall.getLength(),  // length of the original edge
                wallObject.transform.localScale.y * d.dungeonType.wallHeight,  // desired wall height
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
            Debug.DrawRay(p, direction * wall.getLength(), Color.white, 512);
            #endif
            */
        }

        /// <summary>
        /// Remakes walls with a gate and corridor extending outward
        /// </summary>
        /// <param name="d"></param>
        private static List<GameObject> destroyWallsForCorridors(Dungeoneer d) {
            LayerMask wallMask = 1 << 12;
            List<GameObject> wallsToKeep = new List<GameObject>();

            foreach (Edge e in d.spanningTree) {
                float len = (float)(e.getLength());

                Vector3 p = e.p.toVector3AtHeight(d.dungeonType.wallHeight/2);
                Vector3 q = e.q.toVector3AtHeight(d.dungeonType.wallHeight/2);
                Vector3 direction = (q-p).normalized;
                /*
                #if UNITY_EDITOR
                Debug.DrawRay(p, direction * len, Color.blue, 512);
                #endif
                */

                // Cast a sphere to find what walls to destroy
                RaycastHit[] hits = Physics.SphereCastAll(p, d.dungeonType.hallWidth, direction, len, wallMask);
                List<RaycastHit> hit2 = new List<RaycastHit>(hits);
                hit2 = hit2.OrderByDescending(h => h.distance).ToList();
                foreach (RaycastHit hit in hit2) {
                    UnityEngine.Object.Destroy(hit.collider.gameObject);
                }

                // Cast a wider sphere to determine what walls to keep during cleanup
                foreach (RaycastHit hit in Physics.SphereCastAll(p, d.dungeonType.hallWidth * 7, direction, len, wallMask)) {
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
                    UnityEngine.Object.Destroy(w);
                }
            }
        }
    }
}
