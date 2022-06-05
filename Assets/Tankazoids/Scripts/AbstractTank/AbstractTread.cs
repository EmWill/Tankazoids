using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractTread : AbstractPart
{
    public abstract Vector2 HandleMovement(Vector2 directionalInput, bool abilityPressed, Vector2 position, Tank tank);

    public abstract void DecayVelocity(ref Vector2 velocity, Tank tank);
}
