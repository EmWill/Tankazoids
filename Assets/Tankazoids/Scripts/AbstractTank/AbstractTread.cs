using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractTread : AbstractPart
{
    protected Rigidbody2D _tankRigidbody;

    public override void OnEquip(Tank tank)
    {
        base.OnEquip(tank);

        _tankRigidbody = tank.gameObject.GetComponent<Rigidbody2D>();
    }

    public virtual void DecayVelocity()
    {
        if (_tankRigidbody.velocity.magnitude < .25f)
        {
            _tankRigidbody.velocity = Vector3.zero;
        }
        else
        {
            _tankRigidbody.velocity -= _tankRigidbody.velocity * 0.3f * (float)base.TimeManager.TickDelta;
        }
    }

    public virtual void DecayAngularVelocity()
    {
        if (_tankRigidbody.angularVelocity < .25f)
        {
            _tankRigidbody.angularVelocity = 0f;
        }
        else
        {
            _tankRigidbody.angularVelocity -= _tankRigidbody.angularVelocity * 0.3f * (float)base.TimeManager.TickDelta;
        }
    }
}