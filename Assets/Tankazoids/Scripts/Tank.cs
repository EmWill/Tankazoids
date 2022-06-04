using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    public readonly StatManager<float> Health;
    public readonly StatManager<float> MoveSpeed;
    public readonly StatManager<float> RateOfFire;
    public readonly StatManager<int> MaxAmmo;
    public readonly StatManager<float> MaxHeat;
    private int _ammo;
    private float _heat;
    public Tank(float baseHealth, float baseMoveSpeed, float baseRateOfFire, int baseMaxAmmo, float baseMaxHeat)
    {
        Health = new StatManager<float>(baseHealth);
        MoveSpeed = new StatManager<float>(baseMoveSpeed);
        RateOfFire = new StatManager<float>(baseRateOfFire);
        MaxAmmo = new StatManager<int>(baseMaxAmmo);
        MaxHeat = new StatManager<float>(baseMaxHeat);
    }

    
}