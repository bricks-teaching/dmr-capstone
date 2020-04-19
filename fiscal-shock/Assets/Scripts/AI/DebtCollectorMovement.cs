using UnityEngine;
using System.Collections.Generic;
using FiscalShock.Graphs;
using FiscalShock.Pathfinding;
using System.IO;

namespace FiscalShock.AI {
    public class DebtCollectorMovement : MonoBehaviour {
        [Tooltip("The speed at which the object moves.")]
        public float movementSpeed = 3f;

        [Tooltip("The speed at which the object turns.")]
        public float rotationSpeed = 7f;

        [Tooltip("The absolute minimum distance away from the player.")]
        public float safeRadiusMin = 4f;

        [Tooltip("Creates safe radius in case object ends up too close to player.")]
        public float safeRadiusMax = 5f;

        [Tooltip("How close the player needs to be before being pursued.")]
        public float visionRadius = 35f;
        public GameObject player;
        public bool stunned { get; set; }
        public float distanceFromPlayer3D { get; private set; }
        public float distanceFromPlayer2D { get; private set; }
        private CharacterController controller;
        public Cell spawnSite { get; set; }
        private Vertex lastVisitedNode = null;
        private Hivemind hivemind;
        private AStar pathfinder;
        private Stack<Vertex> path;
        private Vector3 nextDestination;
        private Vector3 nextFlatDir;

        void Start() {
            if (player == null) {
                player = GameObject.FindGameObjectWithTag("Player");
            }

            controller = GetComponentInParent<CharacterController>();

            hivemind = GameObject.Find("DungeonSummoner").GetComponent<Hivemind>();
            pathfinder = hivemind.pathfinder;

            foreach(Edge side in spawnSite.sides) {
                // Check that the edge isn't a wall.
                if (!side.isWall) {
                    // Check if the first edge coordinate is out of the ground area.
                    if (!side.p.walkable || side.p.x < hivemind.bounds[0] || side.p.x > hivemind.bounds[1]
                    || side.p.y < hivemind.bounds[2] || side.p.y > hivemind.bounds[3]) {
                        // Check the second edge coordinate.
                        if (!side.q.walkable || side.q.x < hivemind.bounds[0] || side.q.x > hivemind.bounds[1]
                        || side.q.y < hivemind.bounds[2] || side.q.y > hivemind.bounds[3]) {
                            continue;
                        }

                        lastVisitedNode = side.q;
                        break;
                    }

                    lastVisitedNode = side.p;
                    break;
                }
            }

            // DEBUG: Remove or set debug specific.
            // Debug.Log("SPAWN SITE: " + spawnSite.site.vector);
            // Debug.Log("LAST VISITED NODE: " + lastVisitedNode.vector);
        }

        void Update() {
            // Can be stunned, but not hurt. He is immortal. Only death can free you of debt.
            // (Or, ya' know, paying off your debt.)
            if (player == null || stunned) {
                return;
            }

            Vector3 playerDirection = (player.transform.position - transform.position).normalized;
            Vector3 flatPlayerDirection = new Vector3(playerDirection.x, 0, playerDirection.z);
            Vector2 flatPosition = new Vector2(transform.position.x, transform.position.z);
            Vector2 playerFlatPosition = new Vector2(player.transform.position.x, player.transform.position.z);

            Quaternion rotationToPlayer = Quaternion.LookRotation(playerDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotationToPlayer, Time.deltaTime * rotationSpeed);

            distanceFromPlayer3D = Vector3.Distance(player.transform.position, transform.position);
            // Need 2D distance - will only consider how far away enemy is from player on x,z plane.
            distanceFromPlayer2D = Vector2.Distance(playerFlatPosition, flatPosition);

            if (distanceFromPlayer2D < visionRadius) {
                // DEBUG: Remove or set debugging code.
                // Debug.Log("PLAYER WITHIN VISION OF ENEMY.");

                // Unlikely that the path will be valid if player gets away.
                // path = null;

                if (distanceFromPlayer2D > safeRadiusMax) {
                    controller.SimpleMove(flatPlayerDirection * movementSpeed);
                    return;
                }

                if (distanceFromPlayer2D < safeRadiusMin) {
                    controller.SimpleMove(-flatPlayerDirection * movementSpeed);
                    return;
                }

                return;
            }

            if (path == null) {
                path = pathfinder.findPath(lastVisitedNode, hivemind.lastPlayerLocation);

                // DEBUG: Move into debug code or remove.
                StreamWriter writer = new StreamWriter("/home/ybautista/Desktop/new_path.txt");
                Vertex[] pathNodes = path.ToArray();

                foreach (Vertex node in pathNodes) {
                    writer.Write(node.vector + "\n");
                }

                writer.Close();

                // TODO: Handle this better. Right now, could lead to infinite loop,
                // but will be a very rare bug, realistically.
                if (path.Count == 0) {
                    path = null;
                    return;
                }

                lastVisitedNode = path.Pop();

                // DEBUG: Remove or set debugging code.
                Debug.Log("NEXT DESTINATION: " + lastVisitedNode.vector);

                // T: Vector2 -> Vector3
                // TODO: Implement raycasting to go around obstacles
                nextDestination = new Vector3(lastVisitedNode.x, transform.position.y, lastVisitedNode.y);
                Vector3 unnormDirection = nextDestination - transform.position;
                nextFlatDir = new Vector3(unnormDirection.x, 0, unnormDirection.z).normalized;
                controller.SimpleMove(nextFlatDir * movementSpeed);

                // DEBUG: Remove or set debugging code.
                Debug.Log("DIRECTION TO NEXT NODE ON PATH: " + nextFlatDir);
            }

            if (Vector2.Distance(flatPosition, lastVisitedNode.vector) < 0.5) {
                // DEBUG: Remove or set debugging code.
                // Debug.Log("THE DESTINATION HAS BEEN HIT.");

                if (path.Count > 0) {
                    // DEBUG: Remove or set debugging code.
                    Debug.Log("PATH NODE WILL BE REMOVED.");

                    lastVisitedNode = path.Pop();

                    // DEBUG: Remove or set debugging code.
                    Debug.Log("NEXT DESTINATION: " + lastVisitedNode.vector);

                    nextDestination = new Vector3(lastVisitedNode.x, transform.position.y, lastVisitedNode.y);
                    Vector3 unnormDirection = nextDestination - transform.position;
                    nextFlatDir = new Vector3(unnormDirection.x, 0, unnormDirection.z).normalized;
                    controller.SimpleMove(nextFlatDir * movementSpeed);

                    // DEBUG: Remove or set debugging code.
                    Debug.Log("DIRECTION TO NEXT NODE ON PATH: " + nextFlatDir);

                    return;
                }

                path = null;
                return;
            }

            controller.SimpleMove(nextFlatDir * movementSpeed);
        }
    }
}
