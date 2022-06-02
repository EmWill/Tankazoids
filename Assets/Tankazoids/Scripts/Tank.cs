using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    public readonly StatManager<float> Health;
    public readonly StatManager<float> MoveSpeed;
    public readonly StatManager<float> RateOfFire;

    public Tank(float baseHealth, float baseMoveSpeed, float baseRateOfFire)
    {
        Health = new StatManager<float>(baseHealth);
        MoveSpeed = new StatManager<float>(baseMoveSpeed);
        RateOfFire = new StatManager<float>(baseRateOfFire);
    }

    
}
