using FishNet;
using FishNet.Component.ColliderRollback;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class Tank : NetworkBehaviour
{
    public float GetHealth()
    {
        return _health;
    }

    public float GetMaxHealth()
    {
        return maxHealthModifiers.CalculateStat(_bodyComponent.MaxHealth);
    }

    [Server]
    public void AddHealth(float amount)
    {
        if (amount <= 0)
        {
            Debug.LogError("healed 0 or negative!?");
            Debug.LogError("doggy doggy WHAT now!?");
        }

        _health = Math.Min(_health + amount, GetMaxHealth());
    }

    [Server]
    public void RemoveHealth(float amount)
    {
        if (amount <= 0)
        {
            Debug.LogError("took 0 or negative damage!?");
        }

        _health = Math.Max(_health - amount, 0);

        if (_health <= 0)
        {
            Die();
        }
    }

    [Server]
    public void Die()
    {
        if (!_dead && base.IsServer)
        {
            _mapManager.Respawn(this);
            _dead = true;
        }
    }

    public float GetHeat()
    {
        return _heat;
    }

    public float GetMaxHeat()
    {
        return maxHeatModifiers.CalculateStat(_bodyComponent.MaxHeat);
    }


    private void ProcessHeatOnTick()
    {
        if (_sprinting)
        {
            AddHeat(30 * (float)base.TimeManager.TickDelta);
        }
        else
        {
            if (!_overheated)
            {
                RemoveHeat(10 * (float)base.TimeManager.TickDelta);
            }
            else
            {
                RemoveHeat(30 * (float)base.TimeManager.TickDelta);
            }
        }
    }

    // local only!
    private void SetHeat(float amount)
    {
        _heat = amount;

        if (_heat > GetMaxHeat() && !_overheated)
        {
            Overheat();
        }

        if (_heat == 0 && _overheated)
        {
            Unoverheat();
        }
    }

    public void AddHeat(float amount)
    {
        if (amount <= 0)
        {
            Debug.LogError("added 0 or negative heat!?");
        }

        _heat += amount;

        if (_heat > GetMaxHeat() && !_overheated)
        {
            Overheat();
        }
    }

    public void RemoveHeat(float amount)
    {
        if (_heat == 0)
        {
            return;
        }

        if (amount <= 0)
        {
            Debug.LogError("removed 0 or negative heat!?");
        }

        _heat = Math.Max(_heat - amount, 0);

        if (_heat == 0 && _overheated)
        {
            Unoverheat();
        }
    }

    // todo is this ok? theoretically the speedmods should get synced automatically I think ?
    private void Overheat()
    {
        speedModifiers.AddMultiplier(0.25f);
        _overheated = true;
    }

    private void Unoverheat()
    {
        speedModifiers.RemoveMultiplier(0.25f);
        _overheated = false;
    }

    public bool IsOverheated()
    {
        return _overheated;
    }
}
