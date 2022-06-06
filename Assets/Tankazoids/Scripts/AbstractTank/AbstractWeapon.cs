using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractWeapon : AbstractPart
{
    public delegate bool OnSharedCooldownAbilityUseHandler();
    private bool _sharesCooldown;
    public GameObject projectile;
    public event OnSharedCooldownAbilityUseHandler OnSharedCooldownAbilityUse = () => true;
    
    protected float _shotSpeed;
    protected float _timeToLive;
    protected float _baseDamage;

    public override void ActivateAbility(Tank.InputData inputData)
    {
        base.ActivateAbility(inputData);

        if (true && Time.time >= CanUseAt)
        {
            Ability(inputData, _tank);
            CanUseAt = Time.time + (_tank.cooldownModifiers.CalculateStat(_baseCooldown));
        }
    }

    protected virtual void Ability(Tank.InputData inputData, Tank tank)
    {
        Vector3 target = inputData.worldTargetPos - tank.transform.position;
        Vector2 target2D = new(target.x, target.y);
        GameObject proj = Instantiate(projectile, tank.transform.position, Quaternion.FromToRotation(tank.transform.position, inputData.worldTargetPos));
        proj.GetComponent<Projectile>().Shooter = tank;
        Spawn(proj, base.Owner);
        AddForceToProjectile(proj, target2D * _shotSpeed);
        Destroy(proj, _timeToLive);
    }

    [ObserversRpc(BufferLast = true)]
    private void AddForceToProjectile(GameObject proj, Vector2 force)
    {
        var rb = proj.GetComponent<Rigidbody2D>();
        rb.AddForce(force);
    }
}
