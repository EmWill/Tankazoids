using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public bool isRollbackDummy = false;

    public Tank Shooter;
    public float BaseDamage;

    public Vector2 velocity;

    public override void OnStartServer()
    {
        base.OnStartServer();
        InstanceFinder.TimeManager.OnTick += OnTick;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        InstanceFinder.TimeManager.OnTick -= OnTick;
    }

    public void OnTick()
    {
        transform.position += new Vector3(velocity.x, velocity.y, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isRollbackDummy) return;

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
