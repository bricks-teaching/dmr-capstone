using UnityEngine;
using FiscalShock.Graphs;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FiscalShock.Procedural {
    public static class Edgewise {
        public static void generateOnEdges(Dungeoneer d, List<Edge> edges, GameObject thing) {
            float thingLength = thing.GetComponentInChildren<Renderer>().bounds.size.x;
            foreach (Edge e in edges) {
                spawnThingOnEdge(d, e, thing, thingLength);
            }
        }

        /// <summary>
        /// Spawn things along an edge
        /// </summary>
        /// <param name="d"></param>
        /// <param name="edge"></param>
        /// <param name="thing"></param>
        private static void spawnThingOnEdge(Dungeoneer d, Edge edge, GameObject thing, float thingLength) {
            int thingsToSpawn = Mathf.CeilToInt(edge.length / thingLength);
            float lerpDistance = 1 / (float)thingsToSpawn;
            float lerp = 0;
            Vector3 p = edge.p.toVector3AtHeight(0);
            Vector3 q = edge.q.toVector3AtHeight(0);
            Vector3 perpV = Vector3.zero;

            for (int i = 0; i < thingsToSpawn; ++i) {
                lerp += lerpDistance;
                Vector3 where = Vector3.Lerp(p, q, lerp);

                GameObject gro = UnityEngine.Object.Instantiate(thing, where, thing.transform.rotation);

                // The last segment is placed too close to "q," so it ends up
                // not getting rotated. Face the center (0,0,0) instead.
                if (Vector3.Distance(q, where) > 1e-1) {
                    perpV = Vector3.Cross(q - where, Vector3.up).normalized;
                }
                gro.transform.LookAt(where + perpV);
                gro.transform.Rotate(0, 90, 0);

                // It still might not get rotated if the wall segment is 1 tile long
                if (thingsToSpawn == 1) {
                    gro.transform.LookAt(Vector3.zero);
                }
                gro.transform.parent = d.thingOrganizer.transform;
            }
        }
    }
}
