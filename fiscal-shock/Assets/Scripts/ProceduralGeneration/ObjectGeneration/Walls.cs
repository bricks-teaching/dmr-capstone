using UnityEngine;
using FiscalShock.Graphs;
using System.Linq;

namespace FiscalShock.Procedural {
    public static class Walls {
        /// <summary>
        /// Calls all functions to create walls in a dungeon
        /// </summary>
        /// <param name="d"></param>
        public static void setWalls(Dungeoneer d) {
            constructWallsOnRooms(d);
            destroyWallsForCorridors(d);
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

            float actualLength = (float)wall.getLength();
            #if UNITY_EDITOR
            Debug.DrawRay(p, direction * actualLength, Color.white, 512);
            #endif
        }

        /// <summary>
        /// Remakes walls with a gate and corridor extending outward
        /// </summary>
        /// <param name="d"></param>
        public static void destroyWallsForCorridors(Dungeoneer d) {
            float destructionRadius = d.dungeonType.gate.prefab.transform.GetComponent<Renderer>().bounds.extents.x;
            LayerMask wallMask = 1 << 12;
            foreach (Edge e in d.spanningTree) {
                float len = (float)(e.getLength());

                // Clear the center
                Vector3 p = e.p.toVector3AtHeight(1f);
                Vector3 q = e.q.toVector3AtHeight(1f);
                Vector3 direction = (q-p).normalized;
                #if UNITY_EDITOR
                Debug.DrawRay(p, direction * len, Color.blue, 512);
                #endif

                RaycastHit[] hits = Physics.SphereCastAll(p, destructionRadius, direction, len, wallMask);
                foreach (RaycastHit hit in hits) {
                    // destroy all related walls; they are kept underneath an empty gameobject for this purpose
                    UnityEngine.Object.Destroy(hit.collider.gameObject.transform.parent.gameObject);
                }
                if (hits.Length >= 2) { // should always be 2 but just in case
                    Transform wallStart = hits[0].collider.transform;
                    Vector3 startPos = new Vector3(hits[0].point.x, 0, hits[0].point.z);
                    GameObject startGate = UnityEngine.Object.Instantiate(d.dungeonType.gate.prefab, startPos, wallStart.rotation);

                    Vector3 endPos = new Vector3(hits.Last().point.x, 0, hits.Last().point.z);
                    Transform wallEnd = hits.Last().collider.transform;
                    GameObject endGate = UnityEngine.Object.Instantiate(d.dungeonType.gate.prefab, endPos, wallEnd.rotation);

                    Bounds startGateBounds = startGate.transform.GetComponent<Renderer>().bounds;
                    Bounds endGateBounds = endGate.transform.GetComponent<Renderer>().bounds;
/*
                    GameObject firstWall = constructWallOnEdge(new Edge(startGateBounds.center - startGateBounds.extents, endGateBounds.center - endGateBounds.extents));
                    GameObject lastWall = constructWallOnEdge(new Edge( endGateBounds.center + endGateBounds.extents, startGateBounds.center + startGateBounds.extents));

                    // Rotate gate to match hall
                    startGate.transform.LookAt(firstWall.transform);
                    startGate.transform.Rotate(startGate.transform.up, 180);
                    endGate.transform.LookAt(lastWall.transform);
                    endGate.transform.Rotate(endGate.transform.up, 180);
                    */
                }
            }
        }
    }
}
