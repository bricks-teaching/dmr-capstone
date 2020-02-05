using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditSources : MonoBehaviour
{
    //Purpose of this script is to set up initial statistics for creditors, one hostile, one friendly.
    public int maxLoan;
    public int threatLevel;
    public double interestRate;
    public CreditSources(int x, int y, double z) //all values subject to change
    {
        maxLoan = x;
        threatLevel = y; 
        interestRate = z;
    }
    // Start is called before the first frame update
    void Start()
    {
        CreditSources bank = new CreditSources(10000, 0, 5.0);
        CreditSources mob = new CreditSources(4000, 3, 20.3);
    }

    /*
    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
