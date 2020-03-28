using UnityEngine;
using FiscalShock.Graphs;

namespace FiscalShock.Procedural {
    public class WallInfo : MonoBehaviour {
        public Edge associatedEdge;

        private Cheats cheater;

        void Start() {
            cheater = GameObject.Find("Cheater").GetComponent<Cheats>();
        }

        void OnCollisionEnter(Collision col) {
            if (cheater.destroyWalls && col.gameObject.tag == "Bullet" || col.gameObject.tag == "Missile"){
                Destroy(gameObject);
            }
        }
    }
}
