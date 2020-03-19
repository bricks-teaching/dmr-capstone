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
            float floorGridWidth = p.maxX - p.minX;
            float floorGridHeight = p.maxY - p.minY;

            // "Skin" the floor randomly using valid ground tiles
            int tilesPerRow = Mathf.CeilToInt(floorGridWidth / d.dungeonType.groundTileDimensions.x);
            int tilesPerColumn = Mathf.CeilToInt(floorGridHeight / d.dungeonType.groundTileDimensions.z);

            for (int i = -1; i <= tilesPerRow; ++i) {
                for (int j = -1; j <= tilesPerColumn; ++j) {
                    // Randomly select a ground tile to spawn
                    int idx = d.mt.Next(d.dungeonType.groundTiles.Count-1);
                    GameObject tileToSpawn = d.dungeonType.groundTiles[idx].prefab;

                    Vector3 where = new Vector3(
                        (i * d.dungeonType.groundTileDimensions.x) + p.minX,
                        0 + tileToSpawn.transform.position.y,  // ground level is 0, some tiles don't have the right origin
                        (j * d.dungeonType.groundTileDimensions.z) + p.minY
                    );
                    GameObject gro = UnityEngine.Object.Instantiate(tileToSpawn, where, tileToSpawn.transform.rotation);

                    gro.transform.parent = d.groundOrganizer.transform;

                    // Randomly rotate about the y-axis in increments of 90 deg
                    float rotation = d.mt.Next(4) * 90f;
                    gro.transform.Rotate(0, rotation, 0);

                    // Name it for debugging in the editor
                    // does not name uniquely!
                    gro.name = $"{tileToSpawn.name} ({i}, {j})";
                }
            }

            // add a ceiling
            if (d.dungeonType.ceiling != null) {
                float actualWidth = Mathf.Max(Mathf.Abs(p.minX), Mathf.Abs(p.maxX)) * 2.5f;
                float actualHeight = Mathf.Max(Mathf.Abs(p.minY), Mathf.Abs(p.maxY)) * 2.5f;
                constructCeiling(d, actualWidth, actualHeight);
            }
        }

        private static void constructCeiling(Dungeoneer d, float width, float height) {
            GameObject ceiling = UnityEngine.Object.Instantiate(d.dungeonType.ceiling.prefab, new Vector3(0, d.dungeonType.wallHeight, 0), d.dungeonType.ceiling.prefab.transform.rotation);

            ceiling.transform.localScale = new Vector3(
                width,
                height,
                ceiling.transform.localScale.z
            );

            ceiling.transform.parent = d.organizer.transform;
        }
    }
}
