using FishNet.Component.ColliderRollback;
using FishNet.Component.Prediction;
using FishNet.Component.Transforming;
using FishNet.Managing.Timing;
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
        Vector3 target = inputData.worldTargetPos - clientTankPosition;
        Vector2 target2D = new Vector2(target.x, target.y).normalized;

        // rotate direction 90 degrees to get gun offset
        float radian = 90 * Mathf.Deg2Rad;
        float offsetX = target2D.x * Mathf.Cos(radian) - target2D.y * Mathf.Sin(radian);
        float offsetY = target2D.x * Mathf.Sin(radian) + target2D.y * Mathf.Cos(radian);

        // using localscale to determine gun side is a hack
        GameObject proj = Instantiate(projectile, clientTankPosition + 0.2f * Mathf.Sign(transform.localScale.z) * new Vector3(offsetX, offsetY, 0), Quaternion.FromToRotation(clientTankPosition, inputData.worldTargetPos));

        Projectile projectileComponent = proj.GetComponent<Projectile>();

        proj.GetComponent<Rigidbody2D>().velocity = target2D * _shotSpeed;

        if (base.IsServer)
        {
            float rollbackTime = GetRollbackTime(tick);
            projectileComponent.TickForTime(rollbackTime);

            projectileComponent.Shooter = _tank;
            Spawn(proj);

            // the projectile has already existed locally for rollbacktime seconds!
            Destroy(proj, _timeToLive - rollbackTime);
        } else
        {
            Destroy(proj, base.TimeManager.RoundTripTime / 1000f);
            projectileComponent.isRollbackDummy = true;

            // this is local only!!
            proj.GetComponent<Collider2D>().isTrigger = true;
            proj.GetComponent<NetworkObject>().enabled = false;
            proj.GetComponent<PredictedObject>().enabled = false;

            BoxCollider2D myCollider = _tank.GetComponent<BoxCollider2D>();
            Collider2D bulletCollider = proj.GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(myCollider, bulletCollider);
        }

        _tank.AddHeat(5f);
    }

    // ripped from RollbackManager.rollback... yuck!
    public float GetRollbackTime(PreciseTick pt)
    {
        float time;

        // constant from interpolation... should keep this in sync somehow but its private! yuck x2!
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
}
