using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavenTreads : AbstractTread
{
    private bool _reverse = false;
    private float _currBoost = 0f;
    // back in the olden times, when there was a base speed that u insta accelerated to
    //private float _maxBoost = 2f;
    private float _maxBoost = 3f;
    private float _maxReverse = -1.5f;
    private float _accelTime = 2f;
    private float _decelTime = 2f;
    // private float _turnRate = 175f;
    private float _turnRate = 215f;
    private float _minTurnRate = 75f;
    private float _moveRate = 2;
    public List<Sprite> forwardSprites;

    private void adjustSpeed(float targetSpeed)
    {
        //this whole function is the equivalent of shoving everything in your room into the closet. make it more readable and less stupid later
        if (targetSpeed > _currBoost)
        {
            float accelRate = _maxBoost / _accelTime;
            if (_currBoost < 1f)
            {
                accelRate = accelRate * 2f;
            }
            // maybe i also mess with accelRate when you are turning
            _currBoost = Mathf.Min(targetSpeed, _currBoost + accelRate * (float)base.TimeManager.TickDelta);
        }
        else if (targetSpeed < _currBoost)
        {
            float decelRate = _maxBoost / _decelTime;
            if (targetSpeed < 0)
            {
                decelRate = decelRate * 2f;
            }
            else
            {
                decelRate = decelRate / 2f;
            }
            _currBoost = Mathf.Max(targetSpeed, _currBoost - decelRate * (float)base.TimeManager.TickDelta);
        }
    }


    public override void HandleMovement(Tank.InputData inputData)
    {
        float direction = 0;
        if (inputData.directionalInput.x > 0.3f)
            direction = -1;
        else if (inputData.directionalInput.x < -0.3f)
            direction = 1;
        float motion = inputData.directionalInput.y;
        float targetSpeed = 0f;
        if (motion > 0.3f)
        {
            targetSpeed = _maxBoost;
        }
        else if (motion < -0.3f)
        {
            targetSpeed = _maxReverse;
        }

        if (_reverse)
        {
            direction = -direction;
        }
        if (direction != 0)
        {
            float currentTurnRate = _turnRate;
            currentTurnRate += (_minTurnRate - _turnRate) * (Mathf.Abs(_currBoost) / _maxBoost);
            float rotationDistance = currentTurnRate * (float)base.TimeManager.TickDelta;
            _tankRigidbody.SetRotation(_tankRigidbody.rotation + direction * rotationDistance);
        }
        Vector3 retVal;
        adjustSpeed(targetSpeed);
        if (!_reverse)
        {
            //retVal = _tank.transform.position + _tank.transform.up * (_currBoost * _moveRate) * (float)base.TimeManager.TickDelta;
            retVal = _tank.transform.up * (_currBoost * _moveRate);
        }
        else
        {
            //retVal = _tank.transform.position - _tank.transform.up * motion * (_moveRate + _currBoost * _moveRate) * (float)base.TimeManager.TickDelta;
            retVal = _tank.transform.up * motion * (_moveRate + _currBoost * _moveRate);
            retVal -= _tank.transform.up * _currBoost * _moveRate;
        }
        _tankRigidbody.velocity = retVal;
        //_tank.transform.position = retVal;
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