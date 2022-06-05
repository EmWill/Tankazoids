using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWeapon : AbstractWeapon
{
    private const float BASE_DAMAGE = 10f;
    public override float GetCooldown()
    {
        throw new System.NotImplementedException();
    }

    [ServerRpc]
    protected override void Ability(Tank.InputData inputData, Tank tank)
    {
        
        Vector3 target = inputData.worldTargetPos - tank.transform.position;
        GameObject proj = Instantiate(Projectile, tank.transform.position, Quaternion.FromToRotation(tank.transform.position, inputData.worldTargetPos));
        proj.GetComponent<Projectile>().Shooter = tank;
        Spawn(proj, base.Owner);
        var rb = proj.GetComponent<Rigidbody>();
        rb.AddForce(target.normalized * _shotSpeed);
        Destroy(proj, _timeToLive);
    }

    private void Awake()
    {
        _timeToLive = 10f;
        _shotSpeed = 200f;
        _baseCooldown = .5f;
        Projectile.GetComponent<Projectile>().BaseDamage = BASE_DAMAGE;
    }
}
