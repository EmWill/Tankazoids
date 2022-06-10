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
        print("what the hell idiots");
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        print("hell yes brother");
        if (collision.gameObject.TryGetComponent(out Tank tank))
        {
            if (!tank.Equals(Shooter))
            {
                float dmg = Shooter.damageModifiers.CalculateStat(BaseDamage);
                print(dmg + " is the damage");
                tank.RaiseOnHitEvent(ref dmg);
            }
            else
            {
                return;
            }
        }
        Destroy(gameObject);
    }
}
