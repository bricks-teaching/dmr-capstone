using UnityEngine;

public class ShopSign : MonoBehaviour
{
    public Canvas Shop;

    void Start(){
        Shop.enabled = false;
    }

    void onTriggerEnter(){
        Shop.enabled = true;
    }

    void onTriggerExit(){
        Shop.enabled = false;
    }
}
