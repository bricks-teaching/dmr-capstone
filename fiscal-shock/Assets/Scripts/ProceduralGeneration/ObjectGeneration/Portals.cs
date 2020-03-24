using UnityEngine;
using FiscalShock.Graphs;

namespace FiscalShock.Procedural {
    public static class Portals {
        public static void makeDelvePoint(Dungeoneer d) {
            Debug.Log("Placing Delve portal");
            int delveSite = makePortal(d, d.dungeonType.delvePrefab);
            d.validCells[delveSite].spawnedObject.name = "Delve Point";
        }

        public static void makeEscapePoint(Dungeoneer d) {
            Debug.Log("Placing Escape portal");
            int escapeSite = makePortal(d, d.dungeonType.returnPrefab);
            d.validCells[escapeSite].spawnedObject.name = "Escape Point";
        }

        public static int makePortal(Dungeoneer d, GameObject portal) {
            int portalSite = d.mt.Next(d.validCells.Count-1);
            Cell chosenCell = d.validCells[portalSite];

            // Remove any currently spawned objects here
            if (chosenCell.spawnedObject != null) {
                UnityEngine.Object.Destroy(chosenCell.spawnedObject);
            }
            // Also remove neighbors because of clipping issues
            foreach (Cell c in chosenCell.neighbors) {
                if (c.spawnedObject != null) {
                    UnityEngine.Object.Destroy(c.spawnedObject);
                }
            }

            Vector3 where = new Vector3(chosenCell.site.x, d.dungeonType.groundTileDimensions.y, chosenCell.site.y);
            chosenCell.spawnedObject = UnityEngine.Object.Instantiate(portal, where, portal.transform.rotation);
            // Randomly rotate about the y-axis
            float rotation = d.mt.Next(360);
            chosenCell.spawnedObject.transform.Rotate(0, rotation, 0);

            return portalSite;
        }
    }
}
