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

    private float _tickDelta;

    public void Awake()
    {
        InstanceFinder.TimeManager.OnTick += OnTick;
        _tickDelta = (float)InstanceFinder.TimeManager.TickDelta;
    }

    public void OnDestroy()
    {
        InstanceFinder.TimeManager.OnTick -= OnTick;
    }

    private void OnTick()
    {
        TickForTime(_tickDelta);
    }

    public void TickForTime(float time)
    {
        transform.position += new Vector3(velocity.x, velocity.y, 0) * time;
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
