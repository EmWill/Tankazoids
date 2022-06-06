using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractPart : NetworkBehaviour
{
    protected Tank _tank;

    protected float _baseCooldown;
    public float CanUseAt { get; protected set; }
    public virtual void ActivateAbility(Tank.InputData inputData) { }

    public virtual void OnEquip(Tank tank)
    {
        _tank = tank;
    }

    public virtual void OnUnequip()
    {

    }

    // do we really need this ?
    public virtual void OnTankTick(Tank.InputData inputData) {}

    private void Awake()
    {
        CanUseAt = Time.time;
    }

    public abstract float GetCooldown();
}
