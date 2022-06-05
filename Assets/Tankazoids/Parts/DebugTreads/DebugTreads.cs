using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTreads : AbstractTread
{
    public override Vector2 HandleMovement(Vector2 directionalInput, bool abilityPressed, Vector2 position, Tank tank)
    {
        return position + directionalInput;
    }

    public override void DecayVelocity(ref Vector2 velocity, Tank tank) { }

    public override float GetCooldown()
    {
        return 0;
    }
}
