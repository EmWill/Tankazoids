using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractWeapon : AbstractPart
{
    public void ActivateAbility(Tank tank) {}

    public bool SharesCooldownWithOtherWeapon()
    {
        return true;
    }
}
