using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondTreads : AbstractTread
{
    private float _turnRate = 550f;
    private float _moveRate = 9f;

    public override void OnEquip(Tank tank)
    {
        base.OnEquip(tank);

        _tankRigidbody = tank.gameObject.GetComponent<Rigidbody2D>();
    }

    public override Vector3 HandleMovement(Tank.InputData inputData)
    {
        if (inputData.treadsButton.IsPressed())
        {
            float direction = 0;
            if (inputData.directionalInput.x > 0.3f)
                direction = -1;
            else if (inputData.directionalInput.x < -0.3f)
                direction = 1;
            if (direction != 0)
            {
                float currentTurnRate = _turnRate;
                float rotationDistance = currentTurnRate * (float)base.TimeManager.TickDelta;
                _tank.transform.Rotate(0, 0, direction * rotationDistance);
            }
            return new Vector3(0f, 0f, 0f);
        }
        else
        {
            Vector3 retVal;
            retVal = _moveRate * _tank.transform.up;
            return retVal;
            //retVal = _tank.transform.position + _tank.transform.up * _moveRate * (float)base.TimeManager.TickDelta;
          //  _tank.transform.position = retVal;
        }
    }

    public override float GetCooldown()
    {
        return 0;
    }
}