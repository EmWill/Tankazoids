using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public Tank Shooter;
    public float BaseDamage;

    //private void OnCollisionEnter(Collision collision)
    //{

    //    if (collision.gameObject.TryGetComponent(out Tank tank))
    //    {
    //        float dmg = Shooter.damageModifiers.CalculateStat(BaseDamage);
    //        tank.RaiseOnHitEvent(ref dmg);
    //    }
    //    Destroy(gameObject);
    //}
}
