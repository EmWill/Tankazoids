using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractPart : NetworkBehaviour
{
    protected float _baseCooldown;
    public float CanUseAt { get; protected set; }
    public virtual void ActivateAbility(Tank.InputData inputData, Tank tank) { }

    public void Equip(ref Tank tank)
    {

    }

    public void Unequip(ref Tank tank)
    {

    }

    // do we really need this ?
    public void OnTick(ref Tank tank)
    {

    }
    private void Awake()
    {
        CanUseAt = Time.time;
    }

    public abstract float GetCooldown();
}
