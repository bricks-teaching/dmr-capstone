using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewDay : MonoBehaviour
{
    // to be triggered on arrival from the dungeon, accrues interest and causes hostility if not paid
    public bool startNewDay(bool mobNotPaid, bool bankNotPaid){
        if(mobNotPaid){
            //mob threat +1 
        }
        if(bankNotPaid){
            //bank threat +1
        }
        //Increase bank loan by interest rate
        //Increase Mob loan by interest rate
        //set input variables to false so that another payment is required
        return true;
    }
}
