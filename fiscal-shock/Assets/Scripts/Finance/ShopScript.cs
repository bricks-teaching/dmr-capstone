using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ShopScript : MonoBehaviour
{
    public AudioClip paymentSound;
    public AudioClip failureSound;
    private TextMeshProUGUI dialogText;
    private bool hoseBought = false;
    private bool launcherBought = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool buyWeapon(int weapon, float cost){
        if(cost > PlayerFinance.cashOnHand){
            return false;
        }
        PlayerFinance.cashOnHand -= cost;
        if(weapon == 0 && !hoseBought){
            PlayerShoot.slotZero = true;
            hoseBought = true;
        } else if(weapon == 1 && !launcherBought){
            PlayerShoot.slotOne = true;
            launcherBought = true;
        } else {
            return false;
        }
        return true;
    }
}
