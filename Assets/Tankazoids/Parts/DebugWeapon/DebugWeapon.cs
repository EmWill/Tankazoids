using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWeapon : AbstractWeapon
{
    public override float GetCooldown()
    {
        throw new System.NotImplementedException();
    }

    private void Awake()
    {
        _timeToLive = 10f;
        _shotSpeed = 1f;
        _baseCooldown = .5f;
        projectile.GetComponent<Projectile>().BaseDamage = 50f;
    }
}
