using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StatManager
{
    public float Bonus { get; private set; }
    public float Multiplier { get; private set; }

    public StatManager()
    {
        Multiplier = 1f;
        Bonus = 0f;
    }

    public StatManager(float Bonus, float Multiplier)
    {
        this.Bonus = Bonus;
        this.Multiplier = Multiplier;
    }

    public void AddMultiplier(float value)
    {
        Multiplier *= value;
    }

    public void RemoveMultiplier(float value)
    {
        Multiplier /= value;
    }

    public void AddBonus(float value)
    {
        Bonus += value;
    }

    public void RemoveBonus(float value)
    {
        Bonus -= value;
    }

    public float CalculateStat(float value)
    {
        return (value + Bonus) * Multiplier;
    }
}