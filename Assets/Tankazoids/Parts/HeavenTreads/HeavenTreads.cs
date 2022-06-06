using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavenTreads : AbstractTread
{
    private bool _reverse = false;
    private bool _moving = false;
    [SyncVar]
    private float _currBoost = 0f;
    private float _maxBoost = 0.75f;
    private float _accelTime = 1f;
    private float _decelTime = 0.5f;
    private float _turnRate = 175f;
    private float _moveRate = 5;

    private void Awake()
    {
    }

    private void accelerate()
    {
        _currBoost = Mathf.Min(_maxBoost, _currBoost + (_maxBoost / _accelTime) * (float)base.TimeManager.TickDelta);
    }
    private void decelerate()
    {
        _currBoost = Mathf.Max(0f, _currBoost - (_maxBoost / _decelTime) * (float)base.TimeManager.TickDelta);
    }

    public override Vector2 HandleMovement(Vector2 directionalInput, bool abilityPressed, Vector2 position, Tank tank)
    {
        float direction = 0;
        if (directionalInput.x > 0.3f)
            direction = -1;
        else if (directionalInput.x < -0.3f)
            direction = 1;

        bool accelerating = false;
        float motion = directionalInput.y;
        if (motion > 0.3f)
        {
            motion = 1f;
            if (direction == 0)
            {
                accelerate();
                accelerating = true;
            }
        }
        else if (motion < -0.3f)
        {
            motion = -0.75f;
        }
        if (!accelerating)
            decelerate();

        if (_reverse)
        {
            motion = -motion;
            direction = -direction;
        }
        if (direction != 0)
        {
            float rotationDistance = _turnRate * (float)base.TimeManager.TickDelta;
            tank.transform.Rotate(0, 0, direction * rotationDistance);
        }
        Vector2 retVal;
        if (!_reverse)
        {
            retVal = tank.transform.position + tank.transform.up * motion * (_moveRate + _currBoost * _moveRate) * (float)base.TimeManager.TickDelta;
        }
        else
        {
            retVal = tank.transform.position - tank.transform.up * motion * (_moveRate + _currBoost * _moveRate) * (float)base.TimeManager.TickDelta;
        }
        _moving = motion > 0;
        return retVal;

    }

    public override void DecayVelocity(ref Vector2 velocity, Tank tank) { }

    public override float GetCooldown()
    {
        return 0;
    }
}