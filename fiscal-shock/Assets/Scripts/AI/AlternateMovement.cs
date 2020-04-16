using UnityEngine;
using System.Collections.Generic;
using FiscalShock.Pathfinding;
using FiscalShock.Graphs;

namespace FiscalShock.AI {
    public class AlternateMovement : MonoBehaviour {
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
        private EnemyShoot shootScript;
        private CharacterController controller;
        public AnimationManager animationManager;

        /**** FOR INTERNAL USE MAINLY ****/
        public Cell spawnSite { get; set; }
        private Vertex lastVisitedNode = null;
        private Hivemind hivemind;
        private AStar pathfinder;


        void Start() {
            shootScript = GetComponent<EnemyShoot>();

            if (player == null) {
                player = GameObject.FindGameObjectWithTag("Player");
            }

            controller = GetComponent<CharacterController>();

            GameObject dungeonCtrl = GameObject.Find("DungeonSummoner");
            hivemind = dungeonCtrl.GetComponent<Hivemind>();
            pathfinder = hivemind.pathfinder;
       }

        void Update() {
            if (player == null || (Vector3.Distance(player.transform.position, gameObject.transform.position) > visionRadius) || stunned) {
                // TODO drunkard's walk
                animationManager.playIdleAnimation();
                shootScript.spottedPlayer = false;
                return;
            }

            shootScript.spottedPlayer = true;

            // Don't interrupt other animations to play movement
            if (!animationManager.animator.isPlaying) {
                animationManager.playMoveAnimation();
            }

            // This is the only variable that really needs to be a R3 vector - to look in the correct direction.
            Vector3 playerDirection = (player.transform.position - transform.position).normalized;
            Vector3 flatPlayerDirection = new Vector3(playerDirection.x, 0, playerDirection.z).normalized;
            Vector2 flatPosition = new Vector2(transform.position.x, transform.position.z);
            Vector2 playerFlatPosition = new Vector2(player.transform.position.x, player.transform.position.z);

            if (gameObject.tag == "Lobber") {
                playerDirection.y = 0;
            }

            Quaternion rotationToPlayer = Quaternion.LookRotation(playerDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotationToPlayer, rotationSpeed);

            distanceFromPlayer3D = Vector3.Distance(player.transform.position, transform.position);
            // Need 2D distance -new Vector3(playerDirection.x, 0 pl) will only consider how far away enemy is from player on x,z plane.
            distanceFromPlayer2D = Vector2.Distance(playerFlatPosition, flatPosition);

            if (distanceFromPlayer2D > safeRadiusMax) {
                controller.Move(flatPlayerDirection * movementSpeed * Time.deltaTime);
            }

            if (distanceFromPlayer2D < safeRadiusMin) {
                controller.Move(-flatPlayerDirection * movementSpeed * Time.deltaTime);
            }
        }
    }
}
