using FishNet.Component.ColliderRollback;
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

    public override void ActivateAbility(PreciseTick tick, Tank.InputData inputData)
    {
        base.ActivateAbility(tick, inputData);

        if (true && Time.time >= CanUseAt)
        {
            Ability(tick, inputData, _tank);
            CanUseAt = Time.time + (_tank.cooldownModifiers.CalculateStat(_baseCooldown));
        }
    }

    protected virtual void Ability(PreciseTick tick, Tank.InputData inputData, Tank tank)
    {
        base.RollbackManager.Rollback(tick, RollbackManager.PhysicsType.TwoDimensional, base.IsOwner);

        var hitbox = tank.transform.GetChild(0);

        Vector3 target = inputData.worldTargetPos - hitbox.position;
        Vector2 target2D = new Vector2(target.x, target.y).normalized;
        GameObject proj = Instantiate(projectile, hitbox.position, Quaternion.FromToRotation(hitbox.position, inputData.worldTargetPos));
        Spawn(proj, base.Owner);
        Destroy(proj, _timeToLive);

        proj.GetComponent<Projectile>().velocity = target2D * _shotSpeed;

        PrepareProjectile(proj);

        for (uint i = tick.Tick; i < base.RollbackManager.PreciseTick.Tick; i++)
        {
            proj.GetComponent<Projectile>().OnTick();
        }

        base.RollbackManager.Return();
    }

    // we may want to bufferlast here herm... thinking
    [ObserversRpc(RunLocally = true)]
    private void PrepareProjectile(GameObject proj)
    {
        //okay it's possible for a collision to happen before here DESTROYING THE OBJECT! watch out lol
        if (proj != null)
        {
            BoxCollider2D myCollider = _tank.GetComponent<BoxCollider2D>();
            Collider2D bulletCollider = proj.GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(myCollider, bulletCollider);

            proj.GetComponent<Projectile>().Shooter = _tank;

            bulletCollider.enabled = true;
        }
    }
}
