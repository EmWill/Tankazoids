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
        Vector2 target2D = new Vector2(target.x, target.y).normalized;
        GameObject proj = Instantiate(projectile, tank.transform.position, Quaternion.FromToRotation(tank.transform.position, inputData.worldTargetPos));
        proj.GetComponent<Projectile>().Shooter = tank;
        Spawn(proj, base.Owner);
        IgnoreOwnTank(tank, proj);
        AddForceToProjectile(proj, target2D * _shotSpeed);
        Destroy(proj, _timeToLive);

        BoxCollider2D myCollider = tank.GetComponent<BoxCollider2D>();
        Collider2D bulletCollider = proj.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(myCollider, bulletCollider);
    }

    [ObserversRpc]
    private void IgnoreOwnTank(Tank tank, GameObject proj)
    {
        BoxCollider2D myCollider = tank.GetComponent<BoxCollider2D>();
        Collider2D bulletCollider = proj.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(myCollider, bulletCollider);
    }

    [ObserversRpc]
    private void AddForceToProjectile(GameObject proj, Vector2 force)
    {
        //okay it's possible for a collision to happen before here DESTROYING THE OBJECT! watch out lol
        if (proj != null)
        {
            var rb = proj.GetComponent<Rigidbody2D>();
            rb.AddForce(force);
        }
    }
}
