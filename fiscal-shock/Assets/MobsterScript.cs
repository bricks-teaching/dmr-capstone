﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobsterScript : MonoBehaviour
{
    //figure out how to import script interfaces
    //Also need threat increase when not paid, gets bad at 5 and really bad at 8
    public bool mobDue{get; set;} = true;

    public bool addDebt(int amount){
        if(){//mob threat is below 3 and is below max total debt
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
            mobDue = false;
            return true;
        } else { //none of the above
            //display confirmation dialogue
            //reduce debt and money by amount
            mobDue = false;
            return true;
        }
    }
}
