using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractBody : AbstractPart
{
    public abstract int GetMaxHealth();

    public abstract int GetMaxAmmo();

    public abstract int GetMaxHeat();
}
