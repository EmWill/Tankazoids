using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractPart : NetworkBehaviour
{
    public void Equip(ref Tank tank)
    {

    }

    public void Unequip(ref Tank tank)
    {

    }

    // do we really need this ?
    public void OnTick(ref Tank tank)
    {

    }

    public abstract float GetCooldown();
}
