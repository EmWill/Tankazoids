using FishNet.Component.ColliderRollback;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Serializing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractPart : NetworkBehaviour
{
    protected Tank _tank;

    protected float _baseCooldown;
    public float CanUseAt { get; protected set; }
    public virtual void ActivateAbility(PreciseTick tick, Tank.InputData inputData) { }

    public virtual void OnEquip(Tank tank)
    {
        _tank = tank;
    }

    public virtual void OnUnequip()
    {

    }

    // do we really need this ?
    public virtual void OnTankTick(Tank.InputData inputData) {}

    public virtual void HandleMovement(Tank.InputData inputData) {}

    public virtual void GetReconcileData(Writer writer) {}

    public virtual void HandleReconcileData(Reader reader) {}

    private void Awake()
    {
        CanUseAt = Time.time;
    }

    public abstract float GetCooldown();
}
