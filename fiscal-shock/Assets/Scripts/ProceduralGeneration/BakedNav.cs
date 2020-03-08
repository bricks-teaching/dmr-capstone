using UnityEngine.AI;

namespace FiscalShock.Procedural {
    public class BakedNav {
        public NavMeshTriangulation navMeshTriangulation;
        public int navMeshAgentId;

        public BakedNav(int id, NavMeshTriangulation triangulation) {
            navMeshTriangulation = triangulation;
            navMeshAgentId = id;
        }
    }
}
