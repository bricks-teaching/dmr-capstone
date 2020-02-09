//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

interface PlayerF {
    //property signatures
    int money
    {
        get;
        set;
    }

    int debt
    {
        get;
        set;
    }
}

public class PlayerFinance : PlayerF
{
    public int money{get; set;}
    public int debt{get; set;}
    // Start is called before the first frame update
    public PlayerFinance(int x, int y)
    {
        money = x;
        debt = y; //to be changed later, will depend of value of initial equipment
    }

    public PlayerFinance finances = new PlayerFinance(0, 1000);
}
