using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractWeapon : AbstractPart
{
    
    public delegate bool OnSharedCooldownAbilityUseHandler();
    private bool _sharesCooldown;
    public event OnSharedCooldownAbilityUseHandler OnSharedCooldownAbilityUse = () => true;
    public GameObject projectile;
    protected float _shotSpeed;
    protected float _timeToLive;
    protected float _baseDamage;

    public override void ActivateAbility(Tank.InputData inputData, Tank tank)
    {
        base.ActivateAbility(inputData, tank);

        if (true && Time.time >= CanUseAt)
        {
            Ability(inputData, tank);
            CanUseAt = Time.time + (tank.cooldownModifiers.CalculateStat(_baseCooldown));
        }
    }

    protected abstract void Ability(Tank.InputData inputData, Tank tank);
}
