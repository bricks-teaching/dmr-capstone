using UnityEngine;

public class DungeonSign : MonoBehaviour
{
    public Canvas Dungeon;

    void Start(){
        Dungeon.enabled = false;
    }

    void onTriggerEnter(){
        Dungeon.enabled = true;
    }

    void onTriggerExit(){
        Dungeon.enabled = false;
    }
}
