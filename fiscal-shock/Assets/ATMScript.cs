using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATMScript : MonoBehaviour
{
    public bool bankDue = true;

    //figure out how to import script interfaces
    public bool addDebt(int amount){
        if(){//bank threat is below 3 and is below max total debt
            //increase debt by amount
            return true;
        } else {
            return false;
        }
    }

    public bool payDebt(int amount){
        if(){//amount is more than money on hand
            //display a message stating error
            return false;
        } else if(){ //amount is more than the debt
            //display message stating less significant error
            //reduce debt to 0 and money on hand by the debt's value
            bankDue = false;
            return true;
        } else { //none of the above
            //display confirmation dialogue
            //reduce debt and money by amount
            bankDue = false;
            return true;
        }
    }
}
