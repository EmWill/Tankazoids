using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavenTreads : AbstractTread
{
    private bool _reverse = false;
    private bool _moving = false;
    private float _currBoost = 0f;
    private float _maxBoost = 2f;
    private float _accelTime = 2f;
    private float _decelTime = 2f;
    private float _turnRate = 175f;
    private float _minTurnRate = 75f;
    private float _moveRate = 5;
    public List<Sprite> forwardSprites;

    private Rigidbody2D _tankRigidbody;

    private void Awake()
    {
    }

    public override void OnEquip(Tank tank)
    {
        base.OnEquip(tank);

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
        Debug.Log((float)base.TimeManager.TickDelta);

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
           // if (direction == 0)
            //{
                accelerate();
                accelerating = true;
            //}
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
            float currentTurnRate = _turnRate;
            // NEED TO DECELERATE MORE SLOWLY WHEN TURNING FOR THIS TO BE GOOD (or maybe not decelerate AT ALL?)
            currentTurnRate += (_minTurnRate - _turnRate) * (_currBoost / _maxBoost);
            float rotationDistance = currentTurnRate * (float)base.TimeManager.TickDelta;
            _tank.transform.Rotate(0, 0, direction * rotationDistance);
        }
        Vector3 retVal;
        if (!_reverse)
        {
            //retVal = _myTank.transform.position + _myTank.transform.up * motion * (_moveRate + _currBoost * _moveRate) * (float)base.TimeManager.TickDelta;
            retVal = _myTank.transform.position + _myTank.transform.up * ((_moveRate * motion) + _currBoost * _moveRate) * (float)base.TimeManager.TickDelta;
        }
        else
        {
            retVal = _myTank.transform.position - _myTank.transform.up * motion * (_moveRate + _currBoost * _moveRate) * (float)base.TimeManager.TickDelta;
            retVal -= _myTank.transform.up * _currBoost * _moveRate;
        }
        _moving = motion > 0;
        _tank.transform.position = retVal;
    }

    public override void GetReconcileData(Writer writer)
    {
        writer.WriteSingle(_currBoost);
    }

    public override void HandleReconcileData(Reader reader)
    {
        _currBoost = reader.ReadSingle();
    }

    public override float GetCooldown()
    {
        return 0;
    }
}