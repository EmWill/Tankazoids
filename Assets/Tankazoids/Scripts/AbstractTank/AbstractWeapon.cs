using FishNet.Component.ColliderRollback;
using FishNet.Component.Transforming;
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

    [ServerRpc(RunLocally = true)]
    public override void ActivateAbility(PreciseTick tick, Vector3 clientTankPosition, Tank.InputData inputData)
    {
        if (true && Time.time >= CanUseAt)
        {
            Ability(tick, clientTankPosition, inputData);
            CanUseAt = Time.time + (_tank.cooldownModifiers.CalculateStat(_baseCooldown));
        }
    }

    protected virtual void Ability(PreciseTick tick, Vector3 clientTankPosition, Tank.InputData inputData)
    {
        // Vector3 tankPosition = (base.IsServer) ? _tank.oldPositions[tick] : _tank.transform.position;

        Vector3 target = inputData.worldTargetPos - clientTankPosition;
        Vector2 target2D = new Vector2(target.x, target.y).normalized;

        GameObject proj = Instantiate(projectile, clientTankPosition, Quaternion.FromToRotation(clientTankPosition, inputData.worldTargetPos));

        if (base.IsServer)
        {
            Spawn(proj, base.Owner);
            Destroy(proj, _timeToLive);
        } else
        {
            // Destroy(proj, base.TimeManager.RoundTripTime / 1000f);
            proj.GetComponent<Projectile>().isRollbackDummy = true;

            proj.GetComponent<NetworkObject>().enabled = false;
            proj.GetComponent<NetworkTransform>().enabled = false;
        }

        proj.GetComponent<Projectile>().velocity = target2D * _shotSpeed;

        if (base.IsServer)
        {
            PrepareProjectile(proj);

            proj.GetComponent<Projectile>().TickForTime(GetRollbackTime(tick));
        }
    }

    // ripped from RollbackManager.rollback... yuck!
    public float GetRollbackTime(PreciseTick pt)
    {
        float time;

        pt.Tick -= 2;
        uint pastTicks = (base.TimeManager.Tick - pt.Tick);
        //No ticks to rollback to.
        if (pastTicks <= 0)
            return 0;
        //They should never get this high, ever. This is to prevent overflows.
        if (pastTicks > ushort.MaxValue)
            pastTicks = ushort.MaxValue;

        //Weight percent by -40%
        float percent = (float)(pt.Percent / 100f) * -0.4f;
        time = (float)(pastTicks * base.TimeManager.TickDelta);
        time += (float)(percent * base.TimeManager.TickDelta);

        return time;
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
