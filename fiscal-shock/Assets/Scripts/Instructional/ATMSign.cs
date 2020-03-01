using UnityEngine;

public class ATMSign : MonoBehaviour
{
    public Canvas ATM;

    void Start(){
        ATM.enabled = false;
    }

    void onTriggerEnter(){
        ATM.enabled = true;
    }

    void onTriggerExit(){
        ATM.enabled = false;
    }
}
