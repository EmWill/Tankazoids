using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HellTreads : AbstractTread
{
    private bool _reverse = false;
    private bool _moving = false;
    private float _turnRate = 250f;
    private float _moveRate = 5;
    private Transform _transform;

    public Material material1;
    public Material material2;

    private int time = 0;
    private bool isMaterial1;

    public Vector2 HandleMovement(Vector2 directionalInput, bool abilityPressed, Vector2 position)
    {
        if (directionalInput.x != 0 || directionalInput.y != 0)
            print("hel yeah brother");
        Vector2 newDirection = new Vector2(directionalInput.x, directionalInput.y).normalized;
        float motion = Mathf.Min(newDirection.magnitude, 1f);
        float bowAngle = Vector2.SignedAngle(_tank.transform.up, newDirection);
        float sternAngle = Vector2.SignedAngle(-_tank.transform.up, newDirection);
        float angle = bowAngle;
        if (Mathf.Abs(bowAngle) > Mathf.Abs(sternAngle) && (!_moving || sternAngle < 10f))
        {
            angle = sternAngle;
            _reverse = true;
        }
        else if (!_moving || bowAngle < 10f)
        {
            angle = bowAngle;
            _reverse = false;
        }
        if (_reverse)
            angle = sternAngle;
        if (angle != 0)
        {
            float direction;
            if (angle > 0f)
                direction = 1;
            else
                direction = -1;
            if (angle != 0)
            {
                float rotationDistance = Mathf.Min(Mathf.Abs(angle), _turnRate * (float)base.TimeManager.TickDelta);
                _tank.transform.Rotate(0, 0, direction * rotationDistance);
            }
        }
        Vector2 retVal;
        if (!_reverse)
        {
            retVal = _tank.transform.position + _tank.transform.up * motion * _moveRate * (float)base.TimeManager.TickDelta;
        }
        else
        {
            retVal = _tank.transform.position - _tank.transform.up * motion * _moveRate * (float)base.TimeManager.TickDelta;
        }
        _moving = motion > 0;
        return retVal;

    }

    public override float GetCooldown()
    {
        return 0;
    }

    void Update()
    {
        if (_moving)
        {
            time = (time + 1) % 25;

            if (time == 0)
            {
                if (isMaterial1)
                {
                    isMaterial1 = false;
                    gameObject.GetComponent<MeshRenderer>().material = material2;
                }
                else
                {
                    isMaterial1 = true;
                    gameObject.GetComponent<MeshRenderer>().material = material1;
                }
            }
        }
    }
}
