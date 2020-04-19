using UnityEngine;
using System.Collections.Generic;
using FiscalShock.Graphs;
using FiscalShock.Procedural;

namespace FiscalShock.Pathfinding {
    public class Hivemind : MonoBehaviour {
        // Test closest player node
        public Vertex lastPlayerLocation { get; set; }
        public AStar pathfinder { get; private set; }
        internal float[] bounds = new float[4];
        void Start() {
            Voronoi graph = GameObject.Find("DungeonSummoner").GetComponent<Dungeoneer>().navigableGraph;

            GameObject floor = GameObject.Find("Ground");
            findBounds(floor);

            pathfinder = new AStar(graph);
        }

        private void findBounds(GameObject floor) {
            bounds[0] = floor.transform.position.x - floor.transform.localScale.x / 2;
            bounds[1] = floor.transform.position.x + floor.transform.localScale.x / 2;
            bounds[2] = floor.transform.position.y - floor.transform.localScale.y / 2;
            bounds[3] = floor.transform.position.y + floor.transform.localScale.y / 2;
        }
    }
}
