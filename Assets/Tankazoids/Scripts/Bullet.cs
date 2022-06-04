using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float BaseDamage;
    public float BaseSpeed;

    public float Damage { get; set; }
    public float Speed { get; set; }

    public delegate void OnFiredHandler();
    public event OnFiredHandler OnFired;

    public delegate void OnHitHandler(Tank hitee);
    public event OnHitHandler OnHit;

    private void Awake()
    {
        Destroy(gameObject, 10);
    }

    public void Shoot(Vector3 aim)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(aim * Speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Tank target))
        {
            RaiseOnHitEvent(target);
        }
        Destroy(gameObject);
    }

    public void RaiseOnHitEvent(Tank hitee)
    {
        hitee.RaiseOnHitEvent(this);
        OnHit(hitee);
    }
}
