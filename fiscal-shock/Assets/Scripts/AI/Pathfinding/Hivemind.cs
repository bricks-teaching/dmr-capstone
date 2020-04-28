using UnityEngine;
using System.Collections.Generic;
using FiscalShock.Graphs;
using FiscalShock.Procedural;

namespace FiscalShock.Pathfinding {
    public class Hivemind : MonoBehaviour {
        // Test closest player node
        public Vertex lastPlayerLocation { get; set; }
        public AStar pathfinder { get; private set; }

        void Start() {
            Dungeoneer dungeoneer = GameObject.Find("DungeonSummoner").GetComponent<Dungeoneer>();
            Voronoi graph = dungeoneer.navigableGraph;

            pathfinder = new AStar(graph);
        }
    }
}
