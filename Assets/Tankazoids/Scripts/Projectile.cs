using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public bool isRollbackDummy = false;

    [SyncVar]
    public Tank Shooter;

    public float BaseDamage;

    private Rigidbody2D _rigidbody2D;

    // private float _tickDelta;

    public void Awake()
    {
        // InstanceFinder.TimeManager.OnTick += OnTick;
        // _tickDelta = (float)InstanceFinder.TimeManager.TickDelta;

        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        BoxCollider2D myCollider = Shooter.GetComponent<BoxCollider2D>();
        Collider2D bulletCollider = GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(myCollider, bulletCollider);
    }

    public void OnDestroy()
    {
        // InstanceFinder.TimeManager.OnTick -= OnTick;
    }
    /*
    private void OnTick()
    {
        TickForTime(_tickDelta);
    }
    */

    public void TickForTime(float time)
    {
        _rigidbody2D.MovePosition(_rigidbody2D.position + _rigidbody2D.velocity * time);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isRollbackDummy) return;

        Debug.Log(_rigidbody2D.velocity);

        if (collision.gameObject.TryGetComponent(out Projectile projectileComponent))
        {
            if (projectileComponent.Shooter.OwnerId == this.Shooter.OwnerId)
            {
                return;
            }
        }

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
