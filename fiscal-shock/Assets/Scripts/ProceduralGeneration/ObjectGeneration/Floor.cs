using UnityEngine;
using FiscalShock.Graphs;
using System.Linq;
using System.Collections.Generic;

namespace FiscalShock.Procedural {
    public static class Floor {
        public static void setFloor(Dungeoneer d) {
            Debug.Log("Spawning floor");
            IEnumerable<Vertex> vs = d.roomVoronoi.SelectMany(r => r.vertices);
            float mix = vs.Min(v => v.x);
            float max = vs.Max(v => v.x);
            float miy = vs.Min(v => v.y);
            float may = vs.Max(v => v.y);
            Polygon floorever = new Polygon(
                new List<Vertex> {
                    new Vertex(mix, miy),
                    new Vertex(mix, may),
                    new Vertex(max, may),
                    new Vertex(max, miy)
                }
            );

            // already a bounding box because we just made a bounding box
            constructFloorUnderPolygon(d, floorever);
        }

        /// <summary>
        /// Places floor tiles under a polygon.
        /// Should only be used with convex polygons!
        /// </summary>
        /// <param name="d"></param>
        /// <param name="p"></param>
        public static void constructFloorUnderPolygon(Dungeoneer d, Polygon p) {
            float actualWidth = Mathf.Max(Mathf.Abs(p.minX), Mathf.Abs(p.maxX)) * 2.5f;
            float actualHeight = Mathf.Max(Mathf.Abs(p.minY), Mathf.Abs(p.maxY)) * 2.5f;
            // fudge factor on ground cube y to make it line up more nicely
            GameObject flo = stretchCube(d.dungeonType.ground.prefab, actualWidth, actualHeight, -0.2f);
            flo.transform.parent = d.organizer.transform;
            flo.name = "Ground";

            // add optional ceiling
            if (d.dungeonType.ceiling != null) {
                GameObject ceiling = stretchCube(d.dungeonType.ceiling.prefab, actualWidth, actualHeight, d.dungeonType.wallHeight);
                ceiling.transform.parent = d.organizer.transform;
                ceiling.name = "Ceiling";
            }
        }

        private static GameObject stretchCube(GameObject prefab, float width, float height, float yPosition) {
            GameObject qb = UnityEngine.Object.Instantiate(prefab, new Vector3(0, yPosition, 0), prefab.transform.rotation);

            qb.transform.localScale = new Vector3(
                width,
                height,
                qb.transform.localScale.z
            );

            return qb;
        }
    }
}
