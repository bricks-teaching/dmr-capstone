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
            List<Tuple<Vector3, Vector3>> corridorAnchors = destroyWallsForCorridors(d);
            //constructCorridors(d, corridorAnchors);
        }

        public static void constructWallsOnVoronoi(Dungeoneer d) {
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
        public static void constructWallsOnRooms(Dungeoneer d) {
            foreach (VoronoiRoom r in d.roomVoronoi) {
                constructWallsOnPolygon(d, r.exterior);
            }
        }

        /// <summary>
        /// Make walls on all edges of a polygon
        /// </summary>
        /// <param name="d"></param>
        /// <param name="p"></param>
        public static void constructWallsOnPolygon(Dungeoneer d, Polygon p) {
            foreach (Edge e in p.sides) {
                constructWallOnEdge(d, e);
            }
        }

        /// <summary>
        /// Make walls along an arbitrary edge
        /// </summary>
        /// <param name="wall"></param>
        public static void constructWallOnEdge(Dungeoneer d, Edge wall) {
            // Since the prefab is stretched equally along x and y, it must be placed at the center for both x and y
            Vector3 p = wall.p.toVector3AtHeight(d.dungeonType.wallHeight/2);
            Vector3 q = wall.q.toVector3AtHeight(d.dungeonType.wallHeight/2);
            Vector3 wallCenter = (p+q)/2;
            Vector3 direction = (q-p).normalized;

            GameObject wallObject = UnityEngine.Object.Instantiate(d.dungeonType.wall.prefab, wallCenter, d.dungeonType.wall.prefab.transform.rotation);
            wallObject.transform.parent = d.wallOrganizer.transform;

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

            #if UNITY_EDITOR
            Debug.DrawRay(p, direction * wall.getLength(), Color.white, 512);
            #endif
        }

        /// <summary>
        /// Remakes walls with a gate and corridor extending outward
        /// </summary>
        /// <param name="d"></param>
        public static List<Tuple<Vector3, Vector3>> destroyWallsForCorridors(Dungeoneer d) {
            LayerMask wallMask = 1 << 12;
            List<Tuple<Vector3, Vector3>> corridorAnchors = new List<Tuple<Vector3, Vector3>>();

            foreach (Edge e in d.spanningTree) {
                float len = (float)(e.getLength());

                Vector3 p = e.p.toVector3AtHeight(d.dungeonType.wallHeight/2);
                Vector3 q = e.q.toVector3AtHeight(d.dungeonType.wallHeight/2);
                Vector3 direction = (q-p).normalized;
                #if UNITY_EDITOR
                Debug.DrawRay(p, direction * len, Color.blue, 512);
                #endif

                // Cast a ray to find where to cleave the wall
                RaycastHit[] hits = Physics.SphereCastAll(p, d.dungeonType.hallWidth, direction, len, wallMask);
                List<RaycastHit> hit2 = new List<RaycastHit>(hits);
                hit2 = hit2.OrderByDescending(h => h.distance).ToList();
                for (int i = 0; i < hit2.Count; ++i) {
                    RaycastHit hit = hit2[i];
                    Vector3 gateCenter = new Vector3(hit.point.x, 0, hit.point.z);
                    Transform wallStart = hit.collider.transform;

                    // Make two new walls
                    Edge oldEdge = hit.collider.gameObject.GetComponent<WallInfo>().associatedEdge;
                    // if (oldEdge.getLength() > d.dungeonType.hallWidth) {
                    //     /* Need to use linear interpolation to find a point: (b-a)*t
                    //     b: raycast hit point
                    //     a: an original edge endpoint
                    //     t: in [0,1], fraction of edge length when half the width of the gate prefab is subtracted
                    //     */
                    //     Vector3 b = gateCenter;
                    //     Vector3 a = oldEdge.p.toVector3AtHeight(b.y);
                    //     float t = 1 - (d.dungeonType.hallWidth/oldEdge.getLength());
                    //     Vector3 leftEndpoint = Vector3.Lerp(a, b, t);
                    //     constructWallOnEdge(d, new Edge(a, leftEndpoint));

                    //     // Right wall is the same, but using q and not p and moving in the opposite direction
                    //     a = oldEdge.q.toVector3AtHeight(b.y);
                    //     t = 1 - (d.dungeonType.hallWidth/oldEdge.getLength());
                    //     Vector3 rightEndpoint = Vector3.Lerp(b, a, t);
                    //     constructWallOnEdge(d, new Edge(a, rightEndpoint));

                    //     corridorAnchors.Add(new Tuple<Vector3, Vector3>(leftEndpoint, rightEndpoint));
                    // }

                    // Destroy the old wall
                    UnityEngine.Object.Destroy(hit.collider.gameObject);
                }
            }

            return corridorAnchors;
        }

        private static void constructCorridors(Dungeoneer d, List<Tuple<Vector3, Vector3>> corridorEndpointPairs) {
            // TODO fix this
            if (corridorEndpointPairs.Count > 2) {
                UnityEngine.Debug.LogWarning("More than two raycast hits, halls are going to cut through a room");
            }
            for (int i = 0; i < (corridorEndpointPairs.Count-1); i += 2) {
                Edge leftSide = new Edge(corridorEndpointPairs[i].Item1, corridorEndpointPairs[i+1].Item1);
                Edge rightSide = new Edge(corridorEndpointPairs[i].Item2, corridorEndpointPairs[i+1].Item2);

                /*
                if (leftSide.findIntersection(rightSide) == null) {
                    // Swap endpoints if the walls would cross
                    leftSide = new Edge(corridorEndpointPairs[i].Item1, corridorEndpointPairs[i+1].Item2);
                    rightSide = new Edge(corridorEndpointPairs[i].Item2, corridorEndpointPairs[i+1].Item1);
                }
                */

                constructWallOnEdge(d, leftSide);
                constructWallOnEdge(d, rightSide);
            }
        }
    }
}
