using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewDay : MonoBehaviour
{
    // to be triggered on arrival from the dungeon, accrues interest and causes hostility if not paid
    public bool startNewDay(bool mob, bool bank){
        if(!mob){
            //mob threat +1 
        }
        if(!bank){
            //bank threat +1
        }
        //Increase bank loan by interest rate
        //Increase Mob loan by interest rate
        return true;
    }
}
