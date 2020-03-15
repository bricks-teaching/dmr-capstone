using UnityEngine;
using FiscalShock.Demo;

public class Cheats : MonoBehaviour {
    public string teleportToEscapeKey = "f2";
    public string teleportToDelveKey = "f3";
    public string robinHood = "f1";
    public string showMesh = "f4";
    public GameObject player;
    public CharacterController playerController;

    void Update() {
        if (Input.GetKeyDown(teleportToEscapeKey)) {
            GameObject escape = GameObject.Find("Escape Point");
            Vector3 warpPoint = escape.transform.position;
            // Disable controller before teleportation
            playerController.enabled = false;
            player.transform.position = new Vector3(warpPoint.x, warpPoint.y + 25, warpPoint.z);
            playerController.enabled = true;
            Debug.Log($"Teleported to {warpPoint}, 25 units above");
        }
        if (Input.GetKeyDown(teleportToDelveKey)) {
            GameObject delve = GameObject.Find("Delve Point");
            Vector3 warpPoint = delve.transform.position;
            playerController.enabled = false;
            player.transform.position = new Vector3(warpPoint.x, warpPoint.y + 25, warpPoint.z);
            playerController.enabled = true;
            Debug.Log($"Teleported to {warpPoint}, 25 units above");
        }
        if (Input.GetKeyDown(robinHood)) {
            PlayerFinance.cashOnHand += 100;
        }
        if (Input.GetKeyDown(showMesh)) {
            ProceduralMeshRenderer pmr = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<ProceduralMeshRenderer>();
            pmr.enabled = !pmr.enabled;
        }
    }
}
