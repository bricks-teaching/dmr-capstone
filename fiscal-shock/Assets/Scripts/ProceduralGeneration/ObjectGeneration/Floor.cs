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
            Vector2 center = new Vector2(d.currentDungeonType.width/2, d.currentDungeonType.height/2);
            float actualWidth = d.currentDungeonType.width * 1.1f;
            float actualHeight = d.currentDungeonType.width * 1.1f;
            // fudge factor on ground cube y to make it line up more nicely
            GameObject flo = stretchCube(d.currentDungeonType.ground.prefab, actualWidth, actualHeight, -0.2f, center);
            flo.transform.parent = d.organizer.transform;
            flo.name = "Ground";

            // add optional ceiling
            if (d.currentDungeonType.ceiling != null) {
                GameObject ceiling = stretchCube(d.currentDungeonType.ceiling.prefab, actualWidth, actualHeight, d.currentDungeonType.wallHeight, center);
                ceiling.transform.parent = d.organizer.transform;
                ceiling.name = "Ceiling";
            }
        }

        private static GameObject stretchCube(GameObject prefab, float width, float height, float yPosition, Vector2 center) {
            GameObject qb = UnityEngine.Object.Instantiate(prefab, new Vector3(center.x, yPosition, center.y), prefab.transform.rotation);

            qb.transform.localScale = new Vector3(
                width,
                height,
                qb.transform.localScale.z
            );

            return qb;
        }
    }
}
