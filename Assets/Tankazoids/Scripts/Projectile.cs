using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public Tank Shooter;
    public float BaseDamage;

    private void OnCollisionEnter(Collision collision)
    {
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Tank tank))
        {
            if (tank.OwnerId != Shooter.OwnerId)
            {
                // what if the shooter dies... :C
                float dmg = Shooter.damageModifiers.CalculateStat(BaseDamage);
                print(dmg + " is the damage");
                tank.RemoveHealth(dmg);
            }
            else
            {
                return;
            }
        }
        Destroy(gameObject);
    }
}
