using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public readonly StatManager<float> Damage;
    public readonly StatManager<float> Speed;
    public readonly Tank Shooter;

    public delegate void OnFiredHandler();
    public event OnFiredHandler OnFired;

    public delegate void OnHitHandler(Tank hitee);
    public event OnHitHandler OnHit;

    
}
