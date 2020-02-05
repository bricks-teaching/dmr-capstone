//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public class PlayerFinance : MonoBehaviour //(remove parentheses and comment slashes if we need MonoBehavior)
{
    public int money;
    public int debt;
    // Start is called before the first frame update
    public PlayerFinance(int x, int y)
    {
        money = x;
        debt = y; //to be changed later, will depend of value of initial equipment
    }

    void Start()
    {
        PlayerFinance pf = new PlayerFinance(0, 1000);
    }
    /* Unsure if needed, keeping just in case
    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
