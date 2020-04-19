using UnityEngine;
using System.Collections.Generic;
using FiscalShock.Graphs;

namespace FiscalShock.Pathfinding {
    public class PlayerTrigger : MonoBehaviour {
        public Cell cell { get; set; }
        public List<Edge> edges { get; set; }
        private Hivemind hivemind { get; set; }

        void Start() {
            hivemind = GameObject.Find("DungeonSummoner").GetComponent<Hivemind>();
        }

        void OnTriggerEnter(Collider col) {
            if (col.gameObject.layer == 11) {
                // Debug.Log($"Player stepped into {gameObject.name}");
                // Get the first vertex on the first edge and call it good.
                foreach (Edge edge in edges) {
                    if (!edge.isWall) {
                        hivemind.lastPlayerLocation = edge.p;

                        // DEBUG: Remove or set into debugging code.
                        Debug.Log("NEW PLAYER POSITION: " + hivemind.lastPlayerLocation.vector);
                        break;
                    }
                }
            }
        }
    }
}
