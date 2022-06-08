using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavenTreads : AbstractTread
{
    private Tank _myTank;
    private bool _reverse = false;
    private bool _moving = false;

    public float _currBoost = 0f;

    private float _maxBoost = 1.5f;
    private float _accelTime = 1.5f;
    private float _decelTime = 0.5f;
    private float _turnRate = 175f;
    private float _moveRate = 10;

    private Rigidbody2D _tankRigidbody;

    private void Awake()
    {
    }

    public override void OnEquip(Tank tank)
    {
        base.OnEquip(tank);
        _myTank = _tank;

        // _tankRigidbody = tank.gameObject.GetComponent<Rigidbody2D>();
    }

    private void accelerate()
    {
        _currBoost = Mathf.Min(_maxBoost, _currBoost + (_maxBoost / _accelTime) * (float)base.TimeManager.TickDelta);
    }
    private void decelerate()
    {
        _currBoost = Mathf.Max(0f, _currBoost - (_maxBoost / _decelTime) * (float)base.TimeManager.TickDelta);
    }

    public override void HandleMovement(Tank.InputData inputData)
    {
        float direction = 0;
        if (inputData.directionalInput.x > 0.3f)
            direction = -1;
        else if (inputData.directionalInput.x < -0.3f)
            direction = 1;

        bool accelerating = false;
        float motion = inputData.directionalInput.y;
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
            _tank.transform.Rotate(0, 0, direction * rotationDistance);
        }
        Vector2 retVal;
        if (!_reverse)
        {
            retVal = _myTank.transform.position + _myTank.transform.up * motion * (_moveRate + _currBoost * _moveRate) * (float)base.TimeManager.TickDelta;
        }
        else
        {
            retVal = _myTank.transform.position - _myTank.transform.up * motion * (_moveRate + _currBoost * _moveRate) * (float)base.TimeManager.TickDelta;
        }
        _moving = motion > 0;
        _tank.transform.position = retVal;
    }

    public override float GetCooldown()
    {
        return 0;
    }
}